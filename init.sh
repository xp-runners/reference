#!/bin/sh

if [ -z ${TRAVIS_TAG-} ]; then
  VERSION=master
  VCS_TAG=${TRAVIS_COMMIT-$(git log --pretty=format:'%h' --abbrev-commit --date=short -1)}
  echo "Publishing master"
else
  VERSION=${TRAVIS_TAG#v*}
  VCS_TAG=$TRAVIS_TAG
  echo "Publishing $VERSION"
fi
