#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  echo "This is not a build for a tag, abort." >&2
  exit 1
fi

if [ -z "$TMPDIR" ]; then
    TEMPDIR="$TMPDIR"
else
    TEMPDIR="/tmp"
fi

BUILD=$(mktemp -d "$TEMPDIR/tmp.XXXXXXXXXX")
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
  cat $BUILD/$target | perl $BUILD/inline.pl $BUILD > $target
done

# Done
rm -rf $BUILD
ls -al $TARGETS