#!/bin/sh

set -e
set -u

VERSION=${GITHUB_REF#refs/tags/v*}
TARGET=$(pwd)/$1
SETUP=$TARGET/setup-${VERSION}.sh
ENTRYPOINT=$TARGET/xp-run-${VERSION}.sh
TAR=$TARGET/xp-runners_${VERSION}.tar.gz
ZIP=$TARGET/xp-runners_${VERSION}.zip

mkdir -p $TARGET
rm -rf $TAR $ZIP

# One-line installer
cat setup.sh.in | sed -e "s/@VERSION@/$VERSION/g" > $SETUP

# Shell entrypoint
(cat xp-run.sh.in | sed -e "s/@VERSION@/$VERSION/g" ; cat class-main.php ) > $ENTRYPOINT

# Tar.gz for Un*x
tar cvfz $TAR xp.exe xar.exe tput.exe class-main.php web-main.php

# Zipfile for Windows
zip -j $ZIP xp.exe xar.exe tput.exe class-main.php web-main.php

# Done
echo "Downloads"
ls -al $SETUP $ENTRYPOINT $TAR $ZIP