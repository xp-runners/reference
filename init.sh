#!/bin/sh

if [ -z ${TRAVIS_TAG-} ]; then
  VCS_TAG=${TRAVIS_COMMIT-$(git log --pretty=format:'%h' --abbrev-commit --date=short -1)}
  VERSION="snapshot-$VCS_TAG"
  echo "Publishing master ($VCS_TAG)"
else
  VERSION=${TRAVIS_TAG#v*}
  VCS_TAG=$TRAVIS_TAG
  echo "Publishing $VERSION"
fi
