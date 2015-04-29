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

