create or replace type BLOB_TABLE_TYPE as table of blob;
/

SHOW ERRORS

create or replace type VARCHAR2_TABLE_TYPE as table of varchar2(32767);
/

SHOW ERRORS


CREATE OR REPLACE package utl_zipfile as

	function convert_to_blob(p_clob in out nocopy clob) return blob;
	function convert_to_clob(p_blob in out nocopy blob) return clob;

	-- The function will allow for zipping files directly from SQL
	function zip(p_filename in varchar2, p_file in out nocopy blob) return blob;

	-- Append files is the prefered method for use in PLSQL which allows for 
	-- zipping multiple files at the same time. This will create a zip file if
	-- a null reference is used, and append to a zip file if a zipfile exists.
	procedure append_files(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type, p_files in out nocopy blob_table_type);

	-- removes (a) file(s) from a given zip file. A file can be removed by index
	-- or name. Multiple files can be removed by an array of filenames.
	procedure remove_file(p_zipfile in out nocopy blob, p_index in number);
	procedure remove_file(p_zipfile in out nocopy blob, p_filename in varchar2);
	procedure remove_file(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type);

	-- Unzips all the files in a zip file
	procedure unzip(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type, p_files out nocopy blob_table_type);

	-- prefered method for used in PLSQL. This provides methods for returning
	-- clobs in addition to the default blob. Gets the specified file by name
	-- or index
	procedure get_file(p_zipfile in out nocopy blob, p_index in number, p_file out nocopy blob);
	procedure get_file(p_zipfile in out nocopy blob, p_filename in varchar2, p_file out nocopy blob);

	-- Unzips a specific file by name or index from a zipfile. Can be used within 
	-- SQL directly. The file will always come out as a binary.
	function get_file(p_zipfile in out nocopy blob, p_index in number) return blob;
	function get_file(p_zipfile in out nocopy blob, p_filename in varchar2) return blob;

	-- Gets a list of the files contained in a zip file
	function get_files_list(p_zipfile in out nocopy blob) return varchar2_table_type;

end utl_zipfile;
/

SHOW ERRORS

CREATE OR REPLACE AND RESOLVE JAVA SOURCE NAMED "org/pgwc/oracle/util/ZipFileClass" AS
package org.pgwc.oracle.util;

import java.sql.*;
import java.util.*;
import java.io.*;
import java.util.zip.*;
import java.net.*;
import java.util.Date;
import oracle.jdbc.OracleTypes;
import oracle.sql.*;
import oracle.jdbc.driver.*;

public class ZipFileClass
{
	//
	// Zip a single binary file
	//
	public static oracle.sql.BLOB Zip(java.lang.String filename, oracle.sql.BLOB blobPtr) throws Exception 
	{
		//
		// Prepare output streams
		//
		Connection conn = new OracleDriver().defaultConnection();
		oracle.sql.BLOB zipFile    = oracle.sql.BLOB.createTemporary(conn, true, oracle.sql.BLOB.DURATION_CALL);
		ZipOutputStream zipStream  = new ZipOutputStream(zipFile.getBinaryOutputStream());

		//
		// write file
		//
		ZipEntry fileEntry = new ZipEntry(filename);
		zipStream.putNextEntry(fileEntry);
		InputStream fileStream = blobPtr.getBinaryStream();

   	byte data[] = new byte[2048];
		int count;
		while((count = fileStream.read(data, 0, 2048)) != -1) 
		{
			zipStream.write(data, 0, count);
		}

		//
		// Close off
		//
		fileStream.close();
		zipStream.flush();
		zipStream.close();

      return zipFile;
	}

	//
	// Zip multiple files
	//
	public static void AppendFiles(oracle.sql.BLOB[] zipFilePtr, oracle.sql.ARRAY[] filenames, oracle.sql.ARRAY[] blobPtrs) throws Exception 
	{
		//
		// Prepare output streams
		//
		Connection conn            = new OracleDriver().defaultConnection();
		oracle.sql.BLOB zipFile    = oracle.sql.BLOB.createTemporary(conn, true, oracle.sql.BLOB.DURATION_CALL);
		ZipOutputStream zipStream  = new ZipOutputStream(zipFile.getBinaryOutputStream());

		//
		// write new files
		//
		OracleResultSet rsFilenames = (OracleResultSet)filenames[0].getResultSet();
		OracleResultSet rsFiles     = (OracleResultSet)blobPtrs[0].getResultSet();
		for (int i = 0; rsFilenames.next(); i++)
		{
			rsFiles.next();

			// extract filename and file pointer
			java.lang.String filename    = rsFilenames.getString(2);
			oracle.sql.BLOB blobfile     = rsFiles.getBLOB(2);		

			// write entry
			ZipEntry fileEntry = new ZipEntry(filename);
			zipStream.putNextEntry(fileEntry);
			InputStream fileStream = blobfile.getBinaryStream();

			try
			{
   			byte data[] = new byte[2048];
				int count;
				while((count = fileStream.read(data, 0, 2048)) != -1) 
				{
					zipStream.write(data, 0, count);
				}
			}
			finally
			{
				fileStream.close();
			}
		}

		//
		// Close off
		//
		zipStream.flush();
		zipStream.close();

		zipFilePtr[0] = zipFile;
	}

	private static void AddHashKeys(HashMap map, List keys) throws Exception
	{
		for (int i = 0; i < keys.size(); i++) 
		{
			Object key = keys.get(i);
			map.put(key, null);
		}
	}
	
	private static List GetValues(HashMap map, List keys) throws Exception
	{
		List values = new ArrayList();
		for (int i = 0; i < keys.size(); i++) 
		{
			Object key = keys.get(i);
			Object value = map.get(key);
			values.add(value);
		}
		return values;
	}
	
	private static List Convert(oracle.sql.ARRAY[] array) throws Exception
	{
		List list = new ArrayList();
		if (array[0] != null)
		{
			OracleResultSet set = (OracleResultSet)array[0].getResultSet();
			while (set.next()) list.add(set.getObject(2));
		}
		return list;
	}

	private static oracle.sql.BLOB ReadFile(ZipInputStream zipStream) throws Exception
	{
		Connection conn         = new OracleDriver().defaultConnection();
		oracle.sql.BLOB blob    = oracle.sql.BLOB.createTemporary(conn, true, oracle.sql.BLOB.DURATION_CALL);
		OutputStream blobStream = blob.getBinaryOutputStream();
		
		try
		{
			int count;
			byte data[] = new byte[2048];
			while ((count = zipStream.read(data, 0, 2048)) != -1) 
			{
				blobStream.write(data, 0, count);
			}
		}
		finally
		{
			blobStream.close();
		}
		
		return blob;
	}

	private static HashMap GetEntries(List filenames, ZipInputStream zipStream) throws Exception
	{
		HashMap entries = new HashMap();
		AddHashKeys(entries, filenames);
		boolean filesSpecified = (filenames.size() == 0) ? false : true;

		//
		// Read all data from zip file
		//
		ZipEntry entry;
		while((entry = zipStream.getNextEntry()) != null) 
		{
			java.lang.String filename = entry.getName();
			if (filesSpecified)
			{
				if (entries.containsKey(filename))
				{
					oracle.sql.BLOB blob = ReadFile(zipStream);
					entries.put(filename, blob);
				}
			}
			else
			{
				oracle.sql.BLOB blob = ReadFile(zipStream);
				filenames.add(filename);
				entries.put(filename, blob);
			}
		}
		
		return entries;
	}

	public static void Unzip(oracle.sql.BLOB[] zipFile, oracle.sql.ARRAY[] filenames, oracle.sql.ARRAY[] blobPtrs) throws Exception 
	{
		List entryNames = Convert(filenames);
		
		//
		// Open connections and streams
		//
		Connection conn        = new OracleDriver().defaultConnection();
		ZipInputStream zstream = new ZipInputStream(zipFile[0].getBinaryStream());

		// 
		// UnZip specified entries
		//
		HashMap entries;
		try
		{
			entries = GetEntries(entryNames, zstream);
		}
		finally
		{
			zstream.close();
		}
		
		//
		// format for output
		//
		ArrayDescriptor descriptor;
		descriptor = ArrayDescriptor.createDescriptor("VARCHAR2_TABLE_TYPE", conn);
		filenames[0] = new oracle.sql.ARRAY(descriptor, conn, entryNames.toArray());

		descriptor = ArrayDescriptor.createDescriptor("BLOB_TABLE_TYPE", conn);
		List entryData = GetValues(entries, entryNames);
		blobPtrs[0] = new oracle.sql.ARRAY(descriptor, conn, entryData.toArray());
	}
	
	public static oracle.sql.ARRAY ListFiles(oracle.sql.BLOB[] zipFile) throws Exception 
	{
		//
		// Open connections and streams
		//
		Connection conn        = new OracleDriver().defaultConnection();
		ZipInputStream zstream = new ZipInputStream(zipFile[0].getBinaryStream());

		int index = 0;
		byte data[] = new byte[2048];
		List entryNames = new ArrayList();
		ZipEntry entry;

		//
		// Read all data from zip file
		//
		while((entry = zstream.getNextEntry()) != null) 
		{
			entryNames.add(entry.getName());
			index++;
		}

		zstream.close();

		//
		// format for output
		//
		ArrayDescriptor descriptor;
		descriptor = ArrayDescriptor.createDescriptor("VARCHAR2_TABLE_TYPE", conn);
		return new oracle.sql.ARRAY(descriptor, conn, entryNames.toArray());
	}
}
/

SHOW ERRORS

CREATE OR REPLACE package body utl_zipfile as

	function convert_to_blob(p_clob in out nocopy clob) return blob as

		buffer_size constant pls_integer := 500;
		vchar varchar2(500);
		position pls_integer := 1;
		output blob;

	begin

		dbms_lob.createtemporary(output, true);

		for i in 1..ceil(dbms_lob.getlength(p_clob) / buffer_size) loop

			vchar := dbms_lob.substr(p_clob, buffer_size, position);
			dbms_lob.writeappend(output, length(vchar), utl_raw.cast_to_raw(vchar));
			position := position + buffer_size;

		end loop;

		return output;

	end convert_to_blob;

	function convert_to_clob(p_blob in out nocopy blob) return clob as

		c clob;
		vchar varchar2(32767);
		s pls_integer := 1;
		buf pls_integer := 32767;

	begin

		dbms_lob.createtemporary(c, true);

		for i in 1..ceil(dbms_lob.getlength(p_blob) / buf) loop

			vchar := utl_raw.cast_to_varchar2(dbms_lob.substr(p_blob, buf, s));
			dbms_lob.writeappend(c, length(vchar), vchar);
			s := s + buf;

		end loop;

		return c;

	end convert_to_clob;

	function zip(p_filename in varchar2, p_file in out nocopy blob) return blob is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end zip;

	procedure append_files(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type, p_files in out nocopy blob_table_type) 
	as language java name 'org.pgwc.oracle.util.ZipFileClass.AppendFiles(oracle.sql.BLOB[], oracle.sql.ARRAY[], oracle.sql.ARRAY[])';

	procedure remove_file(p_zipfile in out nocopy blob, p_index in number) is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end remove_file;

	procedure remove_file(p_zipfile in out nocopy blob, p_filename in varchar2) is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end remove_file;

	procedure remove_file(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type) is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end remove_file;

	procedure unzip(p_zipfile in out nocopy blob, p_filenames in out nocopy varchar2_table_type, p_files out nocopy blob_table_type) 
	as language java name 'org.pgwc.oracle.util.ZipFileClass.Unzip(oracle.sql.BLOB[], oracle.sql.ARRAY[], oracle.sql.ARRAY[])';

	procedure get_file(p_zipfile in out nocopy blob, p_index in number, p_file out nocopy blob) is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end get_file;

	procedure get_file(p_zipfile in out nocopy blob, p_filename in varchar2, p_file out nocopy blob) is
	
		l_filenames  varchar2_table_type;
		l_files      blob_table_type;
	
	begin

		l_filenames := varchar2_table_type();
		l_filenames.extend();
		l_filenames(1) := p_filename;
		
		unzip(p_zipfile, l_filenames, l_files);
		
		p_file := l_files(1);
		
		if p_file is null then
		
			raise NO_DATA_FOUND;
			
		end if;

	end get_file;

	function get_file(p_zipfile in out nocopy blob, p_index in number) return blob is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end get_file;

	function get_file(p_zipfile in out nocopy blob, p_filename in varchar2) return blob is
	begin

		raise_application_error(-20000, 'Method not implemented');

	end get_file;

	function get_files_list(p_zipfile in out nocopy blob) return varchar2_table_type 
	as language java name 'org.pgwc.oracle.util.ZipFileClass.ListFiles(oracle.sql.BLOB[]) return oracle.sql.ARRAY[]';

end utl_zipfile;
/

SHOW ERRORS

