#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  echo "This is not a build for a tag, abort." >&2
  exit 1
fi

VERSION=${TRAVIS_TAG#v*}
TARGET=$(pwd)/target
ARCHIVE=$TARGET/xp-runners_${VERSION}.tar.gz
SETUP=$TARGET/setup-$VERSION.sh
BINTRAY=$TARGET/generic.config

rm -rf $TARGET
mkdir -p $TARGET
rm -f $SETUP $BINTRAY

cat setup.sh.in | sed -e "s/@VERSION@/$VERSION/g" > $SETUP
tar cvfz $ARCHIVE xp.exe xp class-main.php web-main.php

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
        "includePattern" : "target/(.*)",
        "uploadPattern"  : "\$1"
      }
    ],
    "publish": true
  }
EOF

# Done
ls -al $SETUP $ARCHIVE $BINTRAY