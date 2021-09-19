#!/bin/sh

set -e
set -u 

if [ $GITHUB_RUN_NUMBER -gt 65535 ] ; then
  BUILD_NUM=${GITHUB_RUN_NUMBER:(-5)}
else
  BUILD_NUM=$GITHUB_RUN_NUMBER
fi

case ${GITHUB_REF-} in
  refs/tags/v*)
    RELEASE=${GITHUB_REF#refs/tags/v*}
    ;;

  *)
    RELEASE=$(grep '##' ChangeLog.md | grep -v ???? | head -1 | cut -d ' ' -f 2)
    ;;
esac

VERSION=$RELEASE.$BUILD_NUM
echo "Version $VERSION"

rm -f src/xp.runner/AssemblyInfo.cs.patched
grep -v AssemblyVersion src/xp.runner/AssemblyInfo.cs >> src/xp.runner/AssemblyInfo.cs.patched
echo '[assembly: AssemblyVersion("'$VERSION'")]' >> src/xp.runner/AssemblyInfo.cs.patched
mv src/xp.runner/AssemblyInfo.cs.patched src/xp.runner/AssemblyInfo.cs