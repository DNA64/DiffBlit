﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DiffBlit.Core.Extensions;
using DiffBlit.Core.Utilities;
using Newtonsoft.Json;

namespace DiffBlit.Core.Config
{
    // TODO: patcher version? force update if repo is generated with newer patcher in case of schema changes
    // TODO: validate method that checks for duplicate packages etc.

    /// <summary>
    /// TODO: description
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, IsReference = true)]
    public class Repository
    {
        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public Dictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<Snapshot> Snapshots { get; } = new List<Snapshot>();

        /// <summary>
        /// TODO: description
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public List<Package> Packages { get; } = new List<Package>();

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Repository Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<Repository>(json);
        }

        /// <summary>
        /// TODO: description
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns the matching snapshot contained in the directory, otherwise null.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="progressHandler">The optional handler to report progress to.</param>
        /// <param name="progressStatus">The optional progress status description.</param>
        /// <returns></returns>
        public Snapshot FindSnapshotFromDirectory(string directory, ProgressChangedEventHandler progressHandler = null, string progressStatus = null)
        {
            // searches snapshots by file count in descending order to find the most specific match possible
            Snapshot snapshot = new Snapshot(directory, progressHandler, progressStatus);   // NOTE: this needs to be separated to prevent multiple evaluations
            return Snapshots.OrderByDescending(s => s.Files.Count).FirstOrDefault(s => snapshot.Contains(s));
        }

        /// <summary>
        /// Attempts to locate snapshots that contain a matching file path and contents
        /// </summary>
        /// <param name="basePath">The local absolute content directory.</param>
        /// <param name="repoPath">The repo file path relative to the base directory.</param>
        /// <returns></returns>
        public List<Snapshot> FindSnapshotsFromFile(string basePath, string repoPath)
        {
            List<Snapshot> snapshots = new List<Snapshot>();

            var path = Path.Combine(basePath, repoPath);
            if (!File.Exists(path))
                return null;

            // TODO: while this normally shouldn't be expensive, should probably integrate progress reporting in case you want to deal with a large file
            var hash = Utility.ComputeHash(path);
            foreach (var snapshot in Snapshots)
            {
                foreach (var file in snapshot.FindFileFromHash(hash))
                {
                    if (file.Path.Equals(repoPath) && file.Hash.IsEqual(hash))
                        snapshots.Add(snapshot);
                }
            }

            return snapshots;
        }

        /// <summary>
        /// Attempts to locate a package based on the source and target versions.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Package FindPackage(Version source, Version target)
        {
            return Packages.FirstOrDefault(package => package.SourceSnapshot.Version == source && target == package.TargetSnapshot.Version);
        }

        ///// <summary>
        ///// TODO: description
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public Snapshot GetSnapshot(Guid id)
        //{
        //    return Snapshots.FirstOrDefault(snapshot => snapshot.Id == id);
        //}

        ///// <summary>
        ///// TODO: description
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public Package GetPackage(Guid id)
        //{
        //    return Packages.FirstOrDefault(package => package.Id == id);
        //}

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}