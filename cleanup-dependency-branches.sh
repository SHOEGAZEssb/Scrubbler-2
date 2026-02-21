ORG="Scrubbler-Dev"
REPO_PREFIX="Scrubbler"
BRANCH_PREFIX="chore/bump-deps-"

repos=$(gh repo list "$ORG" --limit 500 --json name -q ".[] | select(.name | startswith(\"$REPO_PREFIX\")) | .name")

for repo in $repos; do
  echo "== $ORG/$repo =="

  branches=$(gh api --paginate "repos/$ORG/$repo/branches?per_page=100" \
    --jq ".[] | select(.name | startswith(\"$BRANCH_PREFIX\")) | .name")

  if [[ -z "$branches" ]]; then
    echo "  no matching branches"
    continue
  fi

  while IFS= read -r branch; do
    [[ -z "$branch" ]] && continue
    echo "  branch: $branch"

    # find an OPEN PR whose head is this branch (within this repo)
    pr_number=$(gh pr list --repo "$ORG/$repo" --state open --head "$branch" --limit 1 --json number -q ".[0].number" 2>/dev/null || true)

    if [[ -n "$pr_number" && "$pr_number" != "null" ]]; then
      echo "    closing PR #$pr_number"
      gh pr close "$pr_number" --repo "$ORG/$repo" >/dev/null 2>&1 || true
    else
      echo "    no open PR found for this branch"
    fi

    echo "    deleting branch (attempt): $branch"
    gh api -X DELETE "repos/$ORG/$repo/git/refs/heads/$branch" >/dev/null 2>&1 || true
  done <<< "$branches"
done