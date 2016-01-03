XP Runners
==========
This is the reference implementation of [XP RFC #0303: Subcommands](https://github.com/xp-framework/rfc/issues/303).

Getting started
---------------
Setup:

```sh
$ cp ~/bin/class-main.php . # From current XP runners
```

Compile:

```sh
$ csc /target:exe /out:xp.exe \
 src\\xp.runner\\*.cs \
 src\\xp.runner\\io\\*.cs \
 src\\xp.runner\\commands\\*.cs \
 src\\xp.runner\\config\\*.cs \
 src\\xp.runner\\exec\\*.cs
```

Test:

```sh
$ ./xp.exe version
XP 7.0.0-dev { PHP 7.0.0 & ZE 3.0.0 } @ Windows NT SLATE 10.0 build 10586 (Windows 10) i586
Copyright (c) 2001-2016 the XP group
FileSystemCL<...\xp\core\src\main\php\>
FileSystemCL<...\xp\core\src\test\php\>
FileSystemCL<...\xp\core\src\main\resources\>
FileSystemCL<...\xp\core\src\test\resources\>
FileSystemCL<...\cygwin\home\Timm\devel\runners\>
```

Commands
--------
The following commands are builtin:

* **version** - Displays version and exits - *also `-v`*
* **eval [code]** - Evaluates code - *also `-e [code]`*
* **write [code]** - Evaluates code, writes result to Console - *also `-w [code]`*
* **dump [code]** - Evaluates code, var_dump()s result - *also `-d [code]`*
* **help** - Displays help - *also `-?`*
* **run** - Runs a class

If no command line arguments are given, the help command is run. If command line arguments are given, but no command is passed, the command defaults to *run*.

Plugin architecture
-------------------
Libraries may provide commands by adding a vendor binary to their composer.json named xp.{vendor}.{name}[.{command}] - for the [unittest](https://github.com/xp-framework/unittest/blob/master/bin/xp.xp-framework.unittest) command, the entry is:

```json
{
    "bin": ["bin/xp.xp-framework.unittest"]
}
```

The `xp.{module}.{ucfirst(command)}Runner` class serves as the entry point (if the command name is omitted, it will simply be `xp.{module}.Runner`).

By installing the package globally, it becomes available in any directory.

```sh
$ composer global require xp-framework/unittest:dev-master
# ...

$ ./xp.exe unittest -e '$this->assertTrue(true)'
[.]

✓: 1/1 run (0 skipped), 1 succeeded, 0 failed
Memory used: 1267.07 kB (1417.43 kB peak)
Time taken: 0.000 seconds
```

*XP Runners know about Composer's filesystem locations and its dependency management. Thus, the above is the short form of `./xp.exe -m /path/to/unittest [-m /path/to/dependencies/of/unittest ...] xp.unittest.Runner ...`*

Execution
---------
There are two execution models:

1. Run once, exit (*this is the default*)
2. Watch a directory, spawn process every time a change occurs (*via `-watch {directory}`*)

The second model is useful e.g. to implement continuous unittest running, see [here](https://github.com/xp-framework/xp-runners/pull/24).

More features
-------------
Less configuration:

* The timezone is automatically mapped from Windows and need no longer be set via `date.timezone`
* If no `use` setting is defined, a globally or locally composer-installed framework will be used

Better Windows integration:

* ANSI color escape sequences are also supported when running in Windows Console
