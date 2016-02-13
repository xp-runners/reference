#!/bin/sh

set -e
set -u

. ./init.sh

temp=${TMPDIR-${TEMP-${TMP-/tmp}}}
BUILD=$(mktemp -d "$temp/tmp.XXXXXXXXXX")
SOURCES="bootstrap.php class-main.php class-path.php scan-path.php web-main.php xar-support.php entry.php"
TARGETS="class-main.php web-main.php"
EXE=$(pwd)/xp.exe
BIN=$(pwd)/xp

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

# Standalone XP
mkbundle -o $BIN $EXE System.Core System.Runtime.Serialization Mono.Posix -z

# Done
rm -rf $BUILD
ls -al $TARGETS