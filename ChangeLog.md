XP Runners change log
=====================

## 7.0.0 / ????-??-??

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