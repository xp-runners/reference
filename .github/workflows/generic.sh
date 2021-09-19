#!/bin/sh

set -e
set -u

VERSION=${GITHUB_REF#refs/tags/v*}
TARGET=$(pwd)/target
SETUP=$TARGET/setup-${VERSION}.sh
ENTRYPOINT=$TARGET/xp-run-${VERSION}.sh
TAR=$TARGET/xp-runners_${VERSION}.tar.gz
ZIP=$TARGET/xp-runners_${VERSION}.zip
PUBLISH=https://xp.baltorepo.com/xp-runners/distribution/upload/

mkdir -p $TARGET
rm -rf $TAR $ZIP

# One-line installer
cat setup.sh.in | sed -e "s/@VERSION@/$VERSION/g" > $SETUP
curl -i \
  --header "Authorization: Bearer ${BALTO_REPO_TOKEN}" \
  --form "download=@${SETUP}" \
  --form "name=installer" \
  --form "version=${VERSION}" \
  --form "description=One-line installer" \
  --form "readme=<README.md" \
  $PUBLISH
echo

# Shell entrypoint
(cat xp-run.sh.in | sed -e "s/@VERSION@/$VERSION/g" ; cat class-main.php ) > $ENTRYPOINT
curl -i \
  --header "Authorization: Bearer ${BALTO_REPO_TOKEN}" \
  --form "download=@${ENTRYPOINT}" \
  --form "name=entrypoint" \
  --form "version=${VERSION}" \
  --form "description=Shell entrypoint" \
  --form "readme=<README.md" \
  $PUBLISH
echo

# Tar.gz for Un*x
tar cvfz $TAR xp.exe xar.exe tput.exe class-main.php web-main.php
curl -i \
  --header "Authorization: Bearer ${BALTO_REPO_TOKEN}" \
  --form "download=@${TAR}" \
  --form "name=tar" \
  --form "version=${VERSION}" \
  --form "description=Gzipped tar archive" \
  --form "readme=<README.md" \
  $PUBLISH
echo

# Zipfile for Windows
zip -j $ZIP xp.exe xar.exe tput.exe class-main.php web-main.php
curl -i \
  --header "Authorization: Bearer ${BALTO_REPO_TOKEN}" \
  --form "download=@${ZIP}" \
  --form "name=zip" \
  --form "version=${VERSION}" \
  --form "description=Zip file" \
  --form "readme=<README.md" \
  $PUBLISH
echo

# Done
echo "Published packages"
ls -al $SETUP $ENTRYPOINT $TAR $ZIP