#!/bin/sh

set -e
set -u

. ./init.sh

if [ master = $VERSION ]; then
  echo "Debian packages are not build for master branch, abort." >&2
  exit 0
fi

BUILD=$(mktemp -d)
ORIGIN=$(pwd)
TARGET=$ORIGIN/target
DEB=$TARGET/xp-runners_${VERSION}-1_all.deb
BINTRAY=$TARGET/debian.config

mkdir -p target
rm -f $DEB $BINTRAY
fakeroot=$(which fakeroot)
cd $BUILD

echo '2.0' > debian-binary

# data.tar.xz
mkdir -p usr/bin

printf '#!/bin/sh\nexec /usr/bin/mono /usr/bin/xp.exe "$@"\n' > usr/bin/xp
chmod 755 usr/bin/xp

printf '#!/bin/sh\nexec /usr/bin/mono /usr/bin/xp.exe ar "$@"\n' > usr/bin/xar
chmod 755 usr/bin/xar

cp $ORIGIN/xp.exe usr/bin/xp.exe
cp $ORIGIN/class-main.php $ORIGIN/web-main.php usr/bin
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
	Depends: php7.1-cli | php7.0-cli | php5-cli, libmono-corlib4.5-cil, libmono-system-core4.0-cil, libmono-system-runtime-serialization4.0-cil
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
	        "deb_architecture" : "i386,amd64",
	        "override"         : 1
	      }
	    }
	  ],
	  "publish": true
	}
EOF

# Done
rm -rf $BUILD
ls -al $DEB $BINTRAY