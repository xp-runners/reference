#!/bin/sh

set -e
set -u

. ./init.sh

TARGET=$(pwd)/target
ARCHIVE=$TARGET/xp-runners_${VERSION}.tar.gz
SETUP=$TARGET/setup-${VERSION}.sh
SLIM=$TARGET/xp-run-${VERSION}.sh
BINTRAY=$TARGET/generic.config

mkdir -p $TARGET
rm -f $SETUP $BINTRAY

cat setup.sh.in | sed -e "s/@VERSION@/$VERSION/g" > $SETUP
tar cvfz $ARCHIVE xp.exe tput.exe class-main.php web-main.php

# Slim runner
(cat xp-run.sh.in | sed -e "s/@VERSION@/$VERSION/g" ; cat class-main.php \
  | sed -e "s/<?php namespace xp;//g" \
  | sed -e "s/xp\\//g" \
  | sed -e "s/stream_wrapper_register/class_alias('xar', 'xp\\\\xar'); stream_wrapper_register/g" \
) > $SLIM

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
      "vcs_tag"  : "${VCS_TAG}",
      "gpgSign"  : true
    },
    "files": [
      {
        "includePattern" : "target/(setup.*sh|xp-run.*sh|xp-runners.*tar.gz)",
        "uploadPattern"  : "\$1",
        "matrixParams"   : { "override": 1 }
      }
    ],
    "publish": true
  }
EOF

# Done
ls -al $SETUP $ARCHIVE $BINTRAY $SLIM