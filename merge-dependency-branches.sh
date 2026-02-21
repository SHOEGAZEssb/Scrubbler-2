ORG="Scrubbler-Dev"
REPO_PREFIX="Scrubbler"
BRANCH_PREFIX="chore/bump-deps-"

# merge method: choose ONE
MERGE_METHOD="--merge"   # or: --squash  or: --rebase

repos=$(gh repo list "$ORG" --limit 500 --json name -q ".[] | select(.name | startswith(\"$REPO_PREFIX\")) | .name")

for repo in $repos; do
  echo "== $ORG/$repo =="

  # find matching branches
  branches=$(gh api --paginate "repos/$ORG/$repo/branches?per_page=100" \
    --jq ".[] | select(.name | startswith(\"$BRANCH_PREFIX\")) | .name")

  if [[ -z "$branches" ]]; then
    echo "  no matching branches"
    continue
  fi

  while IFS= read -r branch; do
    [[ -z "$branch" ]] && continue
    echo "  branch: $branch"

    # get open PR number for this head branch (if any)
    pr_number=$(gh pr list --repo "$ORG/$repo" --state open --head "$branch" --limit 1 --json number -q ".[0].number" 2>/dev/null || true)

    if [[ -z "$pr_number" || "$pr_number" == "null" ]]; then
      echo "    no open PR found for this branch -> delete branch attempt"
      gh api -X DELETE "repos/$ORG/$repo/git/refs/heads/$branch" >/dev/null 2>&1 || true
      continue
    fi

    # inspect mergeability + check status
    pr_json=$(gh pr view "$pr_number" --repo "$ORG/$repo" \
      --json mergeStateStatus,statusCheckRollup,isDraft \
      2>/dev/null || true)

    if [[ -z "$pr_json" ]]; then
      echo "    unable to read PR -> skipping"
      continue
    fi

    is_draft=$(echo "$pr_json" | jq -r '.isDraft')
    merge_state=$(echo "$pr_json" | jq -r '.mergeStateStatus')

    # count failing / pending checks (ignore checks without a conclusion yet)
    failing=$(echo "$pr_json" | jq '[.statusCheckRollup[] | select(.conclusion != null and .conclusion != "SUCCESS" and .conclusion != "SKIPPED" and .conclusion != "NEUTRAL")] | length')
    pending=$(echo "$pr_json" | jq '[.statusCheckRollup[] | select(.conclusion == null)] | length')

    if [[ "$is_draft" == "true" ]]; then
      echo "    PR #$pr_number is draft -> skipping"
      continue
    fi

    # mergeStateStatus values vary; "CLEAN" is the good case
    if [[ "$merge_state" != "CLEAN" ]]; then
      echo "    PR #$pr_number merge state is $merge_state (not CLEAN) -> skipping"
      continue
    fi

    if [[ "$pending" -gt 0 || "$failing" -gt 0 ]]; then
      echo "    PR #$pr_number checks not green (pending=$pending failing=$failing) -> skipping"
      continue
    fi

    echo "    merging PR #$pr_number (checks green) + delete branch"
    gh pr merge "$pr_number" --repo "$ORG/$repo" $MERGE_METHOD --delete-branch >/dev/null 2>&1 || {
      echo "    merge failed (permissions/branch protection/etc.) -> skipping"
      continue
    }

    # fallback delete attempt (in case --delete-branch didn’t do it)
    gh api -X DELETE "repos/$ORG/$repo/git/refs/heads/$branch" >/dev/null 2>&1 || true
    echo "    done"
  done <<< "$branches"
done