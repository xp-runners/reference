#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  echo "This is not a build for a tag, abort." >&2
  exit 1
fi

BUILD=$(mktemp -d)
SOURCES="bootstrap.php class-main.php class-path.php scan-path.php web-main.php xar-support.php entry.php"
TARGETS="class-main.php web-main.php"

# Fetch
for src in $SOURCES ; do
  echo $src
  curl -sSL https://raw.githubusercontent.com/xp-runners/main/master/src/$src > $BUILD/$src
done

# Replace
curl -sSL https://raw.githubusercontent.com/xp-runners/main/master/inline.pl > $BUILD/inline.pl
for target in $TARGETS ; do
  cat $BUILD/$target | perl $BUILD/inline.pl $BUILD
done

# Done
rm -rf $BUILD
ls -al $TARGETS