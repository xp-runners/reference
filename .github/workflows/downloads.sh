#!/bin/sh

set -e
set -u

ORIGIN=$(pwd)
TARGET=$ORIGIN/$1
BUILD=$(mktemp -d)

VERSION=${GITHUB_REF#refs/tags/v*}
SETUP=$TARGET/setup-${VERSION}.sh
ENTRYPOINT=$TARGET/xp-run-${VERSION}.sh
TAR=$TARGET/xp-runners_${VERSION}.tar.gz
ZIP=$TARGET/xp-runners_${VERSION}.zip
DEB=$TARGET/xp-runners_${VERSION}-1_all.deb

mkdir -p $TARGET
rm -rf $TAR $ZIP $DEB

echo "=> One-line installer"
cat setup.sh.in | sed -e "s/@VERSION@/$VERSION/g" > $SETUP

echo "=> Shell entrypoint"
(cat xp-run.sh.in | sed -e "s/@VERSION@/$VERSION/g" ; cat class-main.php ) > $ENTRYPOINT

echo "=> Tar.gz for Un*x"
tar cvfz $TAR xp.exe xar.exe tput.exe class-main.php web-main.php

echo "=> Zipfile for Windows"
zip -j $ZIP xp.exe xar.exe tput.exe class-main.php web-main.php

echo "=> Debian package"
fakeroot=$(which fakeroot || which fakeroot-ng)

# data.tar.xz
mkdir -p $BUILD/usr/bin

printf '#!/bin/sh\nexec /usr/bin/mono /usr/bin/xp.exe "$@"\n' > $BUILD/usr/bin/xp
chmod 755 $BUILD/usr/bin/xp

printf '#!/bin/sh\nexec /usr/bin/mono /usr/bin/xp.exe ar "$@"\n' > $BUILD/usr/bin/xar
chmod 755 $BUILD/usr/bin/xar

cp xp.exe $BUILD/usr/bin/xp.exe
cp class-main.php web-main.php $BUILD/usr/bin

cd $BUILD
$fakeroot tar cfJ data.tar.xz usr/bin/*

# control.tar.gz
size=$(du -k usr/bin | cut -f 1)
echo -n '' > conffiles
cat <<-EOF > control
  Package: xp-runners
  Priority: extra
  Section: devel
  Installed-Size: ${size}
  Maintainer: XP Team <xp-runners@xp-framework.net>
  Architecture: all
  Version: ${VERSION}-1
  Depends: php8.4-cli | php8.3-cli | php8.2-cli | php8.1-cli | php8.0-cli | php7.4-cli, libmono-corlib4.5-cil, libmono-system-core4.0-cil, libmono-system-runtime-serialization4.0-cil
  Provides: xp-runners
  Description: XP Runners
EOF
$fakeroot tar cfz control.tar.gz ./conffiles ./control
cat control

echo '2.0' > debian-binary
ar -q $DEB debian-binary control.tar.gz data.tar.xz
cd $ORIGIN

# Done
echo "=> Downloads"
ls -al $SETUP $ENTRYPOINT $TAR $ZIP $DEB