﻿using System;
using System.IO;
using DiffBlit.Core.Utilities;

namespace DiffBlit.Core.IO
{
    /// <summary>
    /// Provides read-only access to a file.
    /// Currently supported types include local, HTTP(S) and anonymous FTP.
    /// </summary>
    public class ReadOnlyFile : IReadOnlyData
    {
        /// <summary>
        /// The path of the file.
        /// </summary>
        public Uri Path;

        /// <summary>
        /// Provides read-only access to a file located at the specified path.
        /// </summary>
        /// <param name="path"></param>
        public ReadOnlyFile(string path)
        {
            Path = new Uri(path);
        }

        /// <summary>
        /// Opens up a read-only file stream.
        /// </summary>
        public ReadOnlyStream Open()
        {
            if (Path.Scheme == Uri.UriSchemeHttp || Path.Scheme == Uri.UriSchemeHttps ||
                Path.Scheme == Uri.UriSchemeFtp && string.IsNullOrWhiteSpace(Path.UserInfo))
            {
                using (GZipWebClient client = new GZipWebClient())
                {
                    return new ReadOnlyStream(client.OpenRead(Path));
                }
            }

            if (Path.Scheme == Uri.UriSchemeFile)
            {
                return new ReadOnlyStream(new FileStream(Path.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="path"></param>
        public void CopyLocal(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            //if (Path.Scheme == Uri.UriSchemeFile)
            //{

            // make sure the directory exists beforehand
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? throw new InvalidOperationException());


                using (var input = Open())
                using (var output = File.OpenWrite(path))
                {
                    input.CopyTo(output);
                }
            //}
            //else File.Copy(Path.LocalPath, path);
        }

        /// <summary>
        /// Reads all of the file contexts as text.
        /// </summary>
        /// <returns></returns>
        public string ReadAllText()
        {
            using (ReadOnlyStream s = Open())
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
