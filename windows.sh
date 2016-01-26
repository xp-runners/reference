#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  #-DEBUG echo "This is not a build for a tag, abort." >&2
  #-DEBUG exit 1
  TRAVIS_TAG=v7.0.0
fi

VERSION=${TRAVIS_TAG#v*}
EXE=$(pwd)/xp.exe
TARGET=$(pwd)/target
MAIN="$(pwd)/class-main.php $(pwd)/web-main.php"
ZIP=$TARGET/xp-runners_${VERSION}.zip
BINTRAY=$TARGET/windows.config

mkdir -p target
rm -f $ZIP $BINTRAY

# Zipfile for Windows
zip -j $ZIP $EXE $MAIN

# Bintray configuration
date=$(date +%Y-%m-%d)
cat <<-EOF > $BINTRAY
  {
    "package": {
      "name"     : "xp-runners",
      "repo"     : "windows",
      "subject"  : "xp-runners"
    },
    "version": {
      "name"     : "${VERSION}",
      "desc"     : "XP Runners release ${VERSION}",
      "released" : "${date}",
      "vcs_tag"  : "${TRAVIS_TAG}",
      "gpgSign"  : true
    },
    "files": [
      {
        "includePattern" : "target/(.*zip)",
        "uploadPattern"  : "\$1"
      }
    ],
    "publish": true
  }
EOF

# Done
ls -al $ZIP $BINTRAY