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

