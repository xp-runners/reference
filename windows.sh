#!/bin/sh

set -e
set -u

. ./init.sh

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
      "vcs_tag"  : "${VCS_TAG}",
      "gpgSign"  : true
    },
    "files": [
      {
        "includePattern" : "target/(.*zip)",
        "uploadPattern"  : "\$1",
        "matrixParams"   : { "override": 1 }
      }
    ],
    "publish": true
  }
EOF

# Done
ls -al $ZIP $BINTRAY