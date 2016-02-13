#!/bin/sh

set -e
set -u

. ./init.sh

BUILD=$(mktemp -d)
EXE=$(pwd)/xp.exe
BIN=$(pwd)/xp
TARGET=$(pwd)/target
MAIN="$(pwd)/class-main.php $(pwd)/web-main.php"
DEB=$TARGET/xp-runners_${VERSION}-1_all.deb
BINTRAY=$TARGET/debian.config
ASSEMBLIES='System.Core System.Runtime.Serialization Mono.Posix'

mkdir -p target
rm -f $DEB $BINTRAY
fakeroot=$(which fakeroot)
cd $BUILD

echo '2.0' > debian-binary

# data.tar.xz
mkdir -p usr/bin
mkbundle -o $BIN $EXE $ASSEMBLIES -z
cp $MAIN $BIN usr/bin
$fakeroot tar cfJ data.tar.xz usr/bin/*

# control.tar.gz
size=$(du -k $BUILD/usr/bin | cut -f 1)
echo -n '' > conffiles
cat <<-EOF > control
	Package: xp-runners
	Priority: extra
	Section: devel
	Installed-Size: ${size}
	Maintainer: XP Team <xp-runners@xp-framework.net>
	Architecture: all
	Version: ${VERSION}-1
	Depends: php5-cli, libmono-corlib4.5-cil, libmono-2.0-1
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
	      "includePattern" : "target/(xp-runners.*deb)",
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