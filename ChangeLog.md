XP Runners change log
=====================

## ?.?.? / ????-??-??

## 8.1.4 / 2018-09-04

* Fixed `xp-run` compatiblity with non-GNU sed variants, e.g. on Alpine
  (@thekid)

## 8.1.3 / 2018-03-30

* Made compatible with PHP 7.2 on Debian (provided via https://deb.sury.org/)
  (@thekid)

## 8.1.2 / 2018-02-15

* Fixed copyright year - a belated *welcome 2018* - @thekid

## 8.1.1 / 2018-02-15

* Fixed solution message when Mono is not installed - @thekid

## 8.1.0 / 2017-12-16

* Implemented #71: Allow loading extensions by name - @thekid

## 8.0.2 / 2017-12-11

* Fixed issue #72: Installing on Alpine Linux - @thekid

## 8.0.1 / 2017-11-17

* Fixed "undefined variable: $cwd" inside web-main.php - @thekid

## 8.0.0 / 2017-11-12

* Implemented pull request #70: Signalling socket. This allows graceful
  shutdown when being run inside `xp -supervise`.

## 7.9.3 / 2017-09-24

* Made compatible with PHP 7.1 on Debian (provided via https://deb.sury.org/)
  (@thekid)

## 7.9.2 / 2017-07-02 

* Fixed issue #69: Hash marks (#) comments causing exceptions - adjusted
  the ini file parser to simply ignore lines neither containing sections
  nor key/value pairs.
  (@thekid)

## 7.9.1 / 2017-07-02 

* Fixed error message when .NET Framework is not installed. The list
  of dependencies it was suggesting to install was not complete!
  (@thekid)

## 7.9.0 / 2017-06-03

* Reinstated xar command, implementing issue #67 - @thekid
* Refactored entry point code to no longer use `xp::stringOf()`, which
  will be deprecated in XP9. See xp-framework/rfc#324
  (@thekid)

## 7.8.7 / 2017-05-15

* Fixed issue #66: XP running XP - @thekid

## 7.8.6 / 2017-01-16

* Fixed "Call to undefined function raise()" when handling malformed XAR
  archives inside class path
  (@thekid)

## 7.8.5 / 2016-09-26

* Fixed issue #65: Timezone not set correctly - @thekid

## 7.8.4 / 2016-08-15

* Fixed issue #62 ("WSL: Exec format error") by always creating wrapper
  scripts on Linux, MacOS and Unix systems.
  (@thekid)

## 7.8.3 / 2016-08-15

* Changed output arguments and files in stack trace of uncaught exceptions
  during bootstrap to be less verbose - see issue #64
  (@thekid)

## 7.8.2 / 2016-08-14

* Fixed ultra-slim runners to work with xp-framework/core

  - XAR stream wrapper missing: (`Error: Class 'xp\xar' not found`)
  - Bootstrap script unaccesable via `lang.Runtime` class

  See xp-framework/core#157
  (@thekid)

## 7.8.1 / 2016-08-04

* Fixed coloring on Windows 10 redstone 1 - @thekid

## 7.8.0 / 2016-07-24

* Added ultra-slim XP runner `xp-run`. Intended for use inside Travis CI
  and to replace XP runners 6.X. Does not support any of the following:

  - Different execution models
  - Configuration files
  - Subcommands, neither builtin nor as plugins
  - System timezone handling
  - Binary-safe unicode argument handling
  - XP5 entry points (tools/class.php)
  - Class path or module path
  - Handling of "-v", "-e", "-w", "-d" and "-?" shortcuts
  - Composer support

  On the other side, it does not have any dependencies except `/bin/sh`

  Download: https://dl.bintray.com/xp-runners/generic/xp-run-master.sh
  Example: https://github.com/xp-forge/mirrors/blob/master/.travis.yml
  (@thekid)

## 7.7.1 / 2016-07-24

* Fixed timezone handling on non-Windows systems: If system timezone already
  is a valid Olson identifier, use it.
  (@thekid)
* Fixed issue #61: Setup: Symlink errors - @thekid

## 7.7.0 / 2016-07-23

* Implemented xp-framework/rfc#314: Repeated execution model, see PR #60.
  (@thekid)

## 7.6.5 / 2016-07-15

* Fixed issue #59: Depends: php5-cli but it is not installable - @thekid

## 7.6.4 / 2016-07-15

* Fixed issue #58: PHP 5.6: Call to undefined function T::main() - @thekid

## 7.6.3 / 2016-07-11

* Fixed issue #57: Error message improvement - @thekid

## 7.6.2 / 2016-07-04

* Fixed issue #56: Permission denied - @thekid

## 7.6.1 / 2016-07-03

* Fixed supervise execution model not respawning when PHP's exitcode is
  something like `-1073741819` (e.g., because it crashed)
  (@thekid)

## 7.6.0 / 2016-06-20

* Merged PR #54: Remove ANSI escape sequence emulation - @thekid
* Merged PR #55: Add `tput` drop-in replacement - @thekid
* Merged PR #51: Pass `XP_*` environment variables, for example:
  ```
  XP_EXE => "/usr/bin/xp"
  XP_VERSION => "7.5.2.1916"
  XP_MODEL => "default"
  XP_COMMAND => "write"
  ```
  (@thekid)

## 7.5.2 / 2016-06-14

* Fixed issue #52: Shell installer dependencies - @thekid

## 7.5.1 / 2016-06-06

* Fixed issue #50: Problem without HOME  - @thekid

## 7.5.0 / 2016-05-07

* Added support for PHP arguments in `XP_RT` as well as for runtime
  sections inside xp.ini, e.g. `php -d extension_dir=/etc/php`.
  (@thekid)
* Changed configuration to always to take the environment variables
  `XP_RT` (runtime) and `USE_XP` (path to XP core) into account.
  (@thekid)
* Verify configuration specified is valid and give verbose error if not
  (@thekid)
* Changed output for unknown command line arguments from an unhandled
  exception w/ stacktrace to a simple error format
  (@thekid)
* Made `xp -c {path}` display an error when the given file or directory
  does not exist. Previously this might have caused no problem but weird
  behavior during bootstrapping, causing confusion.
  (@thekid)

## 7.4.1 / 2016-03-05

* Merged PR #45: Refactor output handling - @thekid
* Fixed issue #44: System.ArgumentException: Malformed input string `xp.exe` 
  (@thekid)

## 7.4.0 / 2016-02-26

* Fixed xp-runners/main#4: No output for xp -v (PHP7 w/ older XP6 releases)
  (@thekid)
* Implemented PR #42: Control configuration file usage
  . New `-n` command line arg to prevent any xp.ini files from being loaded.
  . New `-c` command line arg to explicitely specifiy an xp.ini file
  (@thekid) 

## 7.3.3 / 2016-02-14

* Fixed module name display in list subcommand on \*nix systems - @thekid
* Removed superfluous ANSI support indirection for non-Windows - @thekid

## 7.3.2 / 2016-02-14

* Fixed #41: Composer dir on XDG Base Directory Specifications - @thekid

## 7.3.1 / 2016-02-14

* Fixed *help* subcommand not passing on rest of command line arguments
  (@thekid)

## 7.3.0 / 2016-02-14

* Merged #40: Implement "xp list", which shows a list of available subcommands
  (@thekid)

## 7.2.4 / 2016-02-13

* Fixed generic setup routine failing on first install - @thekid

## 7.2.3 / 2016-02-13

* Merged #39: No longer create *mkbundle'd* executables - @thekid

## 7.2.2 / 2016-02-13

* Improved message when xp-framework/core cannot be found. See xp-runners/main#2
  (@thekid)
* Merged xp-runners/main#3: Invoke main() method directly instead of using 
  reflection: https://github.com/xp-framework/rfc/issues/298#issuecomment-174314209
  (@thekid)
* Changed build system to publish master builds for generic installer. This
  can be used to run tests, e.g. the specification, without having to first
  tag a release. It also allows installing unstable builds for previewing:
  https://bintray.com/artifact/download/xp-runners/generic/setup-master.sh
  (@thekid)

## 7.2.1 / 2016-02-13

* Added "force" choice in user-interactive mode of https-verified setup
  routine; just like having passed `-f` command line option on startup
  (@thekid)
* Fixed #35: User-interactive mode doesn't work with pipes - @thekid
* Fixed #34: Potential problem when connection terminates mid-stream
  (@thekid)

## 7.2.0 / 2016-02-12

* Merged PR #33: Fix call of mktemp, requires a template on OS X
  (@mikey179, @thekid)
* Optimized entry point scripts by inlining code - @thekid
* Merged xp-runners/main#1: Run scripts without "-e" - @thekid

## 7.1.2 / 2016-02-01

* Fixed issue #32: xp help ambiguity - @thekid
* Fixed issue #31: xp help topics for builtins - @thekid

## 7.1.1 / 2016-01-31

* Fixed `Paths.Binary()` to return the correct results when run with
  mono, standalone on Windows, mkbundle'd binaries and verify this
  works on Windows, Linux and Mac OS X.
  See https://github.com/xp-runners/spec/issues/1#issuecomment-177540328
  (@thekid)

## 7.1.0 / 2016-01-31

* Added support for Mac OS X, see xp-runners/reference#30 - @thekid

## 7.0.0 / 2016-01-31

* Made installable via `apt-get` on Ubuntu and Debian, and provided a one-
  liner "https-verified" installer (see xp-runners/reference#29)
  (@thekid)
* Made path files and `-cp` behave consistently, see xp-runners/reference#11:
  * New `-cp?` argument, which declares an optional class path
  * New `-cp!` argument, which declares an overlay class path
  * Added support for `.php` files in class path
  (@thekid)
* Merged PR #3: Support XP5. The new XP runners also support XP 5.X checkouts
  although **this is to be considered deprecated** (for the discussion, see
  https://github.com/xp-runners/reference/issues/1#issuecomment-168872536)
  (@thekid)
* Added support for ANSI color escape sequences inside Windows Console
  (@thekid)
* Implemented plugin architecture based on Composer and file naming conventions
  `xp.{vendor}.{name}[.{command}]` => `xp.{module}.{ucfirst(command)}Runner`
  (@thekid)
* Added mapping for Windows Timezones to `date.timezone setting`. No more 
  configuration via xp.ini necessary anymore now.
  Inspired by https://github.com/lmcnearney/timezoneinfo-olson-mapper
  (@thekid)
* Initial implementation of xp-framework/rfc#303 with the following commands:
  * **version** - Displays version and exits - *also `-v`*
  * **eval [code]** - Evaluates code - *also `-e [code]`*
  * **write [code]** - Evaluates code, writes result to Console - *also `-w [code]`*
  * **dump [code]** - Evaluates code, var_dump()s result - *also `-d [code]`*
  * **help** - Displays help - *also `-?`*
  * **run** - Runs a class
  * **ar** - Works with XAR archives. See xp-runners/reference#22
  (@thekid)