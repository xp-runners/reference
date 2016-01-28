#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  echo "This is not a build for a tag, abort." >&2
  exit 1
fi

VERSION=${TRAVIS_TAG#v*}
EXE=xp.exe
BIN=xp
MAIN="class-main.php web-main.php"
TARGET=$(pwd)/target
SETUP=$TARGET/setup.sh
BINTRAY=$TARGET/generic.config

mkdir -p target
rm -f $SETUP $BINTRAY

# SFX for HTTPS-Verified install, inspired by http://stackoverflow.com/a/20760267/3074163
(cat setup.sh.in ; tar cvz $EXE $BIN $MAIN) > $SETUP

# Bintray configuration
date=$(date +%Y-%m-%d)
cat <<-EOF > $BINTRAY
  {
    "package": {
      "name"     : "xp-runners",
      "repo"     : "generic",
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
        "includePattern" : "target/(.*sh)",
        "uploadPattern"  : "\$1"
      }
    ],
    "publish": true
  }
EOF

# Done
ls -al $SETUP $BINTRAY