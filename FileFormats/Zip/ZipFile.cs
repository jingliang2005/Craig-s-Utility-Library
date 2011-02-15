﻿/*
Copyright (c) 2011 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.IO;
#endregion

namespace Utilities.FileFormats.Zip
{
    /// <summary>
    /// Helper class for dealing with zip files
    /// </summary>
    public class ZipFile:IDisposable
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="FilePath">Path to the zip file</param>
        /// <param name="Overwrite">Should the zip file be overwritten?</param>
        public ZipFile(string FilePath,bool Overwrite)
        {
            if (string.IsNullOrEmpty(FilePath))
                throw new ArgumentNullException("FilePath");
            ZipFileStream = new FileStream(FilePath, Overwrite ? FileMode.Create : FileMode.OpenOrCreate);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Zip file's FileStream
        /// </summary>
        protected virtual FileStream ZipFileStream { get; set; }

        #endregion

        #region Functions

        /// <summary>
        /// Adds a folder to the zip file
        /// </summary>
        /// <param name="Folder">Folder to add</param>
        public virtual void AddFolder(string Folder)
        {
            if (string.IsNullOrEmpty(Folder))
                throw new ArgumentNullException("Folder");
            if (Folder.EndsWith(@"\"))
                Folder = Folder.Remove(Folder.Length - 1);
            using(Package Package=ZipPackage.Open(ZipFileStream,FileMode.OpenOrCreate))
            {
                List<FileInfo> Files=Utilities.IO.FileManager.FileList(Folder,true);
                foreach(FileInfo File in Files)
                {
                    string FilePath=File.FullName.Replace(Folder,"");
                    AddFile(FilePath, File, Package);
                }
            }
        }

        /// <summary>
        /// Adds a file to the zip file
        /// </summary>
        /// <param name="File">File to add</param>
        public virtual void AddFile(string File)
        {
            if (string.IsNullOrEmpty(File))
                throw new ArgumentNullException("File");
            if (!Utilities.IO.FileManager.FileExists(File))
                throw new ArgumentException("File");
            using (Package Package = ZipPackage.Open(ZipFileStream))
            {
                AddFile(File, new FileInfo(File), Package);
            }
        }

        /// <summary>
        /// Uncompresses the zip file to the specified folder
        /// </summary>
        /// <param name="Folder">Folder to uncompress the file in</param>
        public virtual void UncompressFile(string Folder)
        {
            if (string.IsNullOrEmpty(Folder))
                throw new ArgumentNullException(Folder);
            Utilities.IO.FileManager.CreateDirectory(Folder);
            using (Package Package = ZipPackage.Open(ZipFileStream, FileMode.Open, FileAccess.Read))
            {
                foreach (PackageRelationship Relationship in Package.GetRelationshipsByType("http://schemas.microsoft.com/opc/2006/sample/document"))
                {
                    Uri UriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), Relationship.TargetUri);
                    PackagePart Document = Package.GetPart(UriTarget);
                    Extract(Document, Folder);
                }
                if (File.Exists(Folder + @"\[Content_Types].xml"))
                    File.Delete(Folder + @"\[Content_Types].xml");
            }
        }

        /// <summary>
        /// Extracts an individual file
        /// </summary>
        /// <param name="Document">Document to extract</param>
        /// <param name="Folder">Folder to extract it into</param>
        protected virtual void Extract(PackagePart Document, string Folder)
        {
            if (string.IsNullOrEmpty(Folder))
                throw new ArgumentNullException(Folder);
            string Location = Folder + System.Web.HttpUtility.UrlDecode(Document.Uri.ToString()).Replace('\\', '/');
            Utilities.IO.FileManager.CreateDirectory(Path.GetDirectoryName(Location));
            byte[] Data = new byte[1024];
            using (FileStream FileStream = new FileStream(Location, FileMode.Create))
            {
                Stream DocumentStream=Document.GetStream();
                while (true)
                {
                    int Size = DocumentStream.Read(Data, 0, 1024);
                    FileStream.Write(Data, 0, Size);
                    if (Size != 1024)
                        break;
                }
                FileStream.Close();
            }
        }

        /// <summary>
        /// Adds a file to the zip file
        /// </summary>
        /// <param name="File">File to add</param>
        /// <param name="FileInfo">File information</param>
        /// <param name="Package">Package to add the file to</param>
        protected virtual void AddFile(string File,FileInfo FileInfo,Package Package)
        {
            if (string.IsNullOrEmpty(File))
                throw new ArgumentNullException("File");
            if (!Utilities.IO.FileManager.FileExists(FileInfo.FullName))
                throw new ArgumentException("File");
            Uri UriPath = PackUriHelper.CreatePartUri(new Uri(File, UriKind.Relative));
            PackagePart PackagePart = Package.CreatePart(UriPath, System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Maximum);
            byte[] Data = null;
            Utilities.IO.FileManager.GetFileContents(FileInfo.FullName, out Data);
            PackagePart.GetStream().Write(Data, 0, Data.Count());
            Package.CreateRelationship(PackagePart.Uri, TargetMode.Internal, "http://schemas.microsoft.com/opc/2006/sample/document");
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (ZipFileStream != null)
            {
                ZipFileStream.Close();
                ZipFileStream.Dispose();
                ZipFileStream = null;
            }
        }

        #endregion
    }
}