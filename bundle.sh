#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  echo "This is not a build for a tag, abort." >&2
  exit 1
fi

BUILD=$(mktemp -d)
SOURCES="bootstrap.php class-main.php class-path.php scan-path.php web-main.php xar-support.php"
TARGETS="class-main.php web-main.php"

# Fetch
for src in $SOURCES ; do
  echo $src
  curl -sSL https://raw.githubusercontent.com/xp-runners/main/master/src/$src > $BUILD/$src
done

# Replace
for target in $TARGETS ; do
  cat $BUILD/$target | perl -pe 's^require .(.+).;^open F, "'$BUILD'/$1" or die("$1: $!"); <F> for 1..2; join "", <F>;^ge' > $target
done

# Done
rm -rf $BUILD
ls -al $TARGETS