@echo off
set SRC_DIR=..\source\
set BUILD_DIR=..\build\
set OUTPUT_FILE=%BUILD_DIR%utl_zipfile.sql

spool %SRC_DIR%BLOB_TABLE_TYPE.sql                        >  %OUTPUT_FILE%
spool %SRC_DIR%VARCHAR2_TABLE_TYPE.sql                    >> %OUTPUT_FILE%
spool %SRC_DIR%UTL_ZIPFILE.pks                            >> %OUTPUT_FILE%
spool %SRC_DIR%org.pgwc.oracle.utl.ZipFileClass.jvs       >> %OUTPUT_FILE%
spool %SRC_DIR%UTL_ZIPFILE.pkb                            >> %OUTPUT_FILE%