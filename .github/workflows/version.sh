#!/bin/sh

set -e
set -u 

if [ $GITHUB_RUN_NUMBER -gt 65535 ] ; then
  BUILD=${GITHUB_RUN_NUMBER:(-5)}
else
  BUILD=$GITHUB_RUN_NUMBER
fi

if [ -z ${GITHUB_REF-} ]; then
  RELEASE=$(grep '##' ChangeLog.md | grep -v ???? | head -1 | cut -d ' ' -f 2)
else
  RELEASE=${GITHUB_REF#v*}
fi

echo "Version $RELEASE.$BUILD"

rm -f src/xp.runner/AssemblyInfo.cs.patched
grep -v AssemblyVersion src/xp.runner/AssemblyInfo.cs >> src/xp.runner/AssemblyInfo.cs.patched
echo '[assembly: AssemblyVersion("'$RELEASE'.'$BUILD'")]' >> src/xp.runner/AssemblyInfo.cs.patched
mv src/xp.runner/AssemblyInfo.cs.patched src/xp.runner/AssemblyInfo.cs