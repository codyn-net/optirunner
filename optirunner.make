

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(CMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG"
ASSEMBLY = bin/Debug/optirunner.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =
BUILD_DIR = bin/Debug

OPTIRUNNER_EXE_MDB_SOURCE=bin/Debug/optirunner.exe.mdb
OPTIRUNNER_EXE_MDB=$(BUILD_DIR)/optirunner.exe.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(CMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize-
ASSEMBLY = bin/Release/optirunner.exe
ASSEMBLY_MDB =
COMPILE_TARGET = exe
PROJECT_REFERENCES =
BUILD_DIR = bin/Release

OPTIRUNNER_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(OPTIRUNNER_EXE_MDB)

BINARIES = \
	$(OPTIRUNNER)


RESGEN=resgen2

all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES)

FILES = \
	Optimization.Runner/Application.cs \
	Optimization.Runner/InitialPopulation.cs \
	Optimization.Runner/Database.cs \
	Optimization.Runner.Console/Visual.cs \
	Optimization.Runner.Console/Application.cs \
	Optimization.Runner.Console/AssemblyInfo.cs

DATA_FILES =

RESOURCES =

EXTRAS = \
	optirunner.in

REFERENCES = \
	Mono.Data.Sqlite \
	System.Data \
	$(OPTIMIZATION_SHARP_LIBS)

DLL_REFERENCES =

CLEANFILES = $(PROGRAMFILES) $(BINARIES)

include $(top_srcdir)/Makefile.include
OPTIRUNNER = $(BUILD_DIR)/optirunner

$(eval $(call emit-deploy-wrapper,OPTIRUNNER,optirunner,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

install-data-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		$(INSTALL) -c -m 0755 $$ASM $(DESTDIR)$(pkglibdir); \
		$(INSTALL) -c -m 0755 $$ASM.mdb $(DESTDIR)$(pkglibdir); \
	done;

uninstall-hook:
	for ASM in $(INSTALLED_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`; \
		rm -f $(DESTDIR)$(pkglibdir)/`basename $$ASM`.mdb; \
	done;
