dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT(optirunner, 2.23)
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROGS(CMCS, dmcs gmcs, no)

if test "x$CMCS" = "xno"; then
	AC_MSG_ERROR([dmcs nor gmcs found])
fi

AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=YES]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=NO]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_DEBUG, true)
	enable_debug=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([OPTIMIZATION_SHARP], [optimization-sharp-2.0 >= 2.24])

dnl find assembly paths for unstable installed assemblies that will be
dnl copied into monoskel installation directory for local usage with the app
UNSTABLE_INSTALL_ASSEMBLIES="optimization-sharp-2.0 biorob-math-sharp-1.0"
INSTALLED_ASSEMBLIES="`$PKG_CONFIG --variable=Libraries $UNSTABLE_INSTALL_ASSEMBLIES`"
AC_SUBST(INSTALLED_ASSEMBLIES)

dnl package checks, per config


AC_CONFIG_FILES([
optirunner
Makefile

])

AC_OUTPUT
