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
TARGET=$(pwd)/xp-runners_${VERSION}-1_all.deb

fakeroot=$(which fakeroot)

rm -f $TARGET
cd $BUILD

# debian-binary
echo '2.0' > debian-binary

# data.tar.xz
mkdir -p usr/bin
mkbundle -o $BUILD/usr/bin/xp $EXE --deps -z
$fakeroot tar cfJ data.tar.xz usr/bin/xp

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

# .deb package
ar -q $TARGET debian-binary control.tar.gz data.tar.xz

# Done
rm -rf $BUILD
ls -al $TARGET