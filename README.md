# Mesh27Slice

3D mesh 27-slicing tool for unity

Click [youtube link](https://youtu.be/eCTSH2cvoi8) for walkthrough

## Repository Setup

This repository uses a fork-based workflow.

- `origin` points to the GitHub fork: `git@github.com:thedrhax14/blattodea.git`
- `upstream` points to the original source repository and is fetch-only
- local development happens on `main`

Check the current remotes:

```bash
git remote -v
```

Expected shape:

```bash
origin    git@github.com:thedrhax14/blattodea.git (fetch)
origin    git@github.com:thedrhax14/blattodea.git (push)
upstream  ssh://amrullah@blattodea-git/srv/likerepo.git (fetch)
upstream  DISABLED (push)
```

## Normal Flow

Start work from the local `main` branch:

```bash
git checkout main
git pull
```

Make changes, then commit and push to the fork:

```bash
git add <files>
git commit -m "Describe the change"
git push
```

## Syncing From The Original Repository

The original repository still uses `master`, so sync from `upstream/master` into local `main`.

Fetch upstream changes:

```bash
git fetch upstream
```

Merge them into `main`:

```bash
git checkout main
git merge upstream/master
git push
```

If you prefer a linear history, use rebase instead of merge:

```bash
git checkout main
git fetch upstream
git rebase upstream/master
git push --force-with-lease
```

## Git LFS

This repository uses Git LFS for large binary assets.

Install Git LFS once per machine:

```bash
git lfs install
```

Clone normally after Git LFS is installed:

```bash
git clone git@github.com:thedrhax14/blattodea.git
```

Useful verification command:

```bash
git lfs fsck
```

## Notes

- Push only to `origin`
- Do not push to `upstream`
- `main` is the working branch on the fork
- `upstream/master` is the source branch for incoming updates

### [LICENSE](/LICENSE)