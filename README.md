XP Runners
==========

Setup:

```sh
$ cp ~/bin/class-main.php . # From current XP runners
```

Compile:

```sh
$ csc /target:exe /out:xp.exe src\\*.cs src\\commands\\*.cs
```

Test:

```sh
$ ./xp.exe version
XP 6.9.2-dev { PHP 7.0.0 & ZE 3.0.0 } @ Windows NT SLATE 10.0 build 10586 (Windows 10) i586
Copyright (c) 2001-2015 the XP group
FileSystemCL<...\xp\core\src\main\php\>
FileSystemCL<...\xp\core\src\test\php\>
FileSystemCL<...\xp\core\src\main\resources\>
FileSystemCL<...\xp\core\src\test\resources\>
FileSystemCL<...\cygwin\home\Timm\devel\runners\>
```

Supported command line options
------------------------------
The following command line options are supported as short versions of the commands:

* `-v`: version - Displays version and exits
* `-e`: eval - Evaluates code
* `-w`: write - Evaluates code, writes result to Console
* `-d`: dump - Evaluates code, var_dump()s result
* `-?`: help - Displays help

The default command is *run*.

Plugins
-------

```sh
$ composer global require xp-framework/unittest:dev-master
# ...

$ ./xp.exe unittest -e '$this->assertTrue(true)'
[.]

✓: 1/1 run (0 skipped), 1 succeeded, 0 failed
Memory used: 1267.07 kB (1417.43 kB peak)
Time taken: 0.000 seconds
```