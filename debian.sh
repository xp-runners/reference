#!/bin/sh

set -e
set -u

if [ -z ${TRAVIS_TAG-} ]; then
  #-DEBUG echo "This is not a build for a tag, abort." >&2
  #-DEBUG exit 1
  TRAVIS_TAG=v7.0.0
fi

VERSION=${TRAVIS_TAG#v*}
BUILD=$(mktemp -d)
EXE=$(pwd)/xp.exe
TARGET=$(pwd)/target
MAIN="$(pwd)/class-main.php $(pwd)/web-main.php"
DEB=$TARGET/xp-runners_${VERSION}-1_all.deb
BINTRAY=$TARGET/debian.config

mkdir -p target
rm -f $DEB $BINTRAY
fakeroot=$(which fakeroot)
cd $BUILD

echo '2.0' > debian-binary

# data.tar.xz
mkdir -p usr/bin
cp $MAIN usr/bin
mkbundle -o $BUILD/usr/bin/xp $EXE --deps -z
$fakeroot tar cfJ data.tar.xz usr/bin/*

# control.tar.gz
size=$(stat -c '%s' $BUILD/usr/bin/xp)
echo -n '' > conffiles
cat <<-EOF > control
	Package: xp-runners
	Priority: extra
	Section: devel
	Installed-Size: ${size}
	Maintainer: XP Team <xp-runners@xp-framework.net>
	Architecture: all
	Version: ${VERSION}-1
	Depends: php5-cli, mono-runtime | libmono-2.0-1
	Provides: xp-runners
	Description: XP Runners
EOF
$fakeroot tar cfz control.tar.gz conffiles control
cat control

ar -q $DEB debian-binary control.tar.gz data.tar.xz

# Bintray configuration
date=$(date +%Y-%m-%d)
cat <<-EOF > $BINTRAY
	{
	  "package": {
	    "name"     : "xp-runners",
      "repo"     : "debian",
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
	      "includePattern" : "(.*deb)",
	      "uploadPattern"  : "\$1",
	      "matrixParams"   : {
	        "deb_distribution" : "jessie",
	        "deb_component"    : "main",
	        "deb_architecture" : "i386,amd64"
	      }
	    }
	  ],
	  "publish": true
	}
EOF

# Done
rm -rf $BUILD
ls -al $DEB $BINTRAY