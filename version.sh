#!/bin/sh

set -e
set -u 

if [ $TRAVIS_BUILD_NUMBER -gt 65535 ] ; then
  BUILD=${TRAVIS_BUILD_NUMBER:(-5)}
else
  BUILD=$TRAVIS_BUILD_NUMBER
fi

if [ -z ${TRAVIS_TAG-} ]; then
  RELEASE=$(grep '##' ChangeLog.md | grep -v ???? | head -1 | cut -d ' ' -f 2)
else
  RELEASE=${TRAVIS_TAG#v*}
fi

echo "Version $RELEASE.$BUILD"

rm -f src/xp.runner/AssemblyInfo.cs.patched
grep -v AssemblyVersion src/xp.runner/AssemblyInfo.cs >> src/xp.runner/AssemblyInfo.cs.patched
echo '[assembly: AssemblyVersion("'$RELEASE'.'$BUILD'")]' >> src/xp.runner/AssemblyInfo.cs.patched
mv src/xp.runner/AssemblyInfo.cs.patched src/xp.runner/AssemblyInfo.cs