using System;
using System.IO;

namespace AngryWasp.DirectoryScanner
{
    /// <summary>
    /// Defines the action on a directory which triggered the event
    /// </summary>
    public enum ScanDirectoryAction
    {
        /// <summary>
        /// Enter a directory
        /// </summary>
        Enter,

        /// <summary>
        /// Leave a directory
        /// </summary>
        Leave
    }

    #region Event argument definition for ScanDirectory.FileEvent

    /// <summary>
    /// Information about the file in the current directory.
    /// </summary>
    public class FileEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Block the default constructor.
        /// </summary>
        private FileEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryEventArgs"/> class.
        /// </summary>
        /// <param name="fileInfo"><see cref="FileInfo"/> object for the current file.</param>
        internal FileEventArgs(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException("fileInfo");

            // Get File information
            _fileInfo = fileInfo;
        }

        #endregion Constructors

        #region Properties

        private bool _cancel;
        private FileInfo _fileInfo;

        /// <summary>
        /// Gets the current file information.
        /// </summary>
        /// <value>The <see cref="FileInfo"/> object for the current file.</value>
        public FileInfo Info
        {
            get { return _fileInfo; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the directory scan.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the scan must be cancelled; otherwise, <see langword="false"/>.
        /// </value>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        #endregion Properties
    }

    #endregion Event argument definition for ScanDirectory.FileEvent

    #region Event argument definition for ScanDirectory.DirectoryEvent

    /// <summary>
    /// Event arguments for the DirectoryEvent
    /// </summary>
    public class DirectoryEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        /// Block the default constructor.
        /// </summary>
        private DirectoryEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryEventArgs"/> class.
        /// </summary>
        /// <param name="directory"><see cref="DirectoryInfo"/> object for the current path.</param>
        /// <param name="action">The action.</param>
        internal DirectoryEventArgs(DirectoryInfo directory, ScanDirectoryAction action)
        {
            if (directory == null) throw new ArgumentNullException("directory");

            // Get File information
            _directoryInfo = directory;
            _action = action;
        }

        #endregion Constructors

        #region Properties

        private DirectoryInfo _directoryInfo;
        private ScanDirectoryAction _action;
        private bool _cancel;

        /// <summary>
        /// Gets the current directory information.
        /// </summary>
        /// <value>The <see cref="DirectoryInfo"/> object for the current directory.</value>
        public DirectoryInfo Info
        {
            get { return _directoryInfo; }
        }

        /// <summary>
        /// Gets the current directory action.
        /// </summary>
        /// <value>The <see cref="ScanDirectoryAction"/> action value.</value>
        public ScanDirectoryAction Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to cancel the directory scan.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the scan must be cancelled; otherwise, <see langword="false"/>.
        /// </value>
        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        #endregion Properties
    }

    #endregion Event argument definition for ScanDirectory.DirectoryEvent

    /// <summary>
    /// Scan directory trees
    /// </summary>
    public class DirectoryScanner
    {
        private const string _patternAllFiles = "*.*";

        #region Handling of the FileEvent

        /// <summary>
        /// Definition for the FileEvent.
        ///	</summary>
        public delegate void FileEventHandler(object sender, FileEventArgs e);

        /// <summary>
        /// Event is raised for each file in a directory.
        /// </summary>
        public event FileEventHandler FileEvent;

        /// <summary>
        /// Raises the file event.
        /// </summary>
        /// <param name="fileInfo"><see cref="FileInfo"/> object for the current file.</param>
        private bool RaiseFileEvent(FileInfo fileInfo)
        {
            bool continueScan = true;

            // Create a new argument object for the file event.
            FileEventArgs args = new FileEventArgs(fileInfo);

            // Now raise the event.
            FileEvent(this, args);

            continueScan = !args.Cancel;

            return continueScan;
        }

        #endregion Handling of the FileEvent

        #region Handling of the DirectoryEvent

        /// <summary>
        /// Definition for the DirectoryEvent.
        /// </summary>
        public delegate void DirectoryEventHandler(object sender, DirectoryEventArgs e);

        /// <summary>
        /// Event is raised for each directory.
        /// </summary>
        public event DirectoryEventHandler DirectoryEvent;

        /// <summary>
        /// Raises the directory event.
        /// </summary>
        /// <param name="directory"><see cref="DirectoryInfo"/> object for the current path.</param>
        /// <param name="action">The <see cref="ScanDirectoryAction"/> action value.</param>
        /// <returns><see langword="true"/> when the scan is allowed to continue. <see langword="false"/> if otherwise;</returns>
        private bool RaiseDirectoryEvent(DirectoryInfo directory, ScanDirectoryAction action)
        {
            bool continueScan = true;

            // Only do something when the event has been declared.
            if (FileEvent != null)
            {
                // Create a new argument object for the file event.
                DirectoryEventArgs args = new DirectoryEventArgs(directory, action);

                // Now raise the event.
                DirectoryEvent(this, args);

                continueScan = !args.Cancel;
            }
            return continueScan;
        }

        #endregion Handling of the DirectoryEvent

        #region Public methods

        /// <summary>
        /// Walks the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><see langword="true"/> when the scan finished without being interupted. <see langword="false"/> if otherwise;</returns>
        public bool WalkDirectory(string path)
        {
            // Validate path argument.
            if (path == null || path.Length == 0) throw new ArgumentNullException("path");

            return this.WalkDirectory(new DirectoryInfo(path));
        }

        /// <summary>
        /// Walks the specified directory.
        /// </summary>
        /// <param name="directory"><see cref="DirectoryInfo"/> object for the current path.</param>
        /// <returns><see langword="true"/> when the scan finished without being interupted. <see langword="false"/> if otherwise;</returns>
        public bool WalkDirectory(DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException("directory");
            }

            return this.WalkDirectories(directory);
        }

        #endregion Public methods

        #region Overridable methods

        /// <summary>
        /// Processes the directory.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <param name="action">The action.</param>
        /// <returns><see langword="true"/> when the scan is allowed to continue. <see langword="false"/> if otherwise;</returns>
        public virtual bool ProcessDirectory(DirectoryInfo directoryInfo, ScanDirectoryAction action)
        {
            if (DirectoryEvent != null)
            {
                return RaiseDirectoryEvent(directoryInfo, action);
            }
            return true;
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns><see langword="true"/> when the scan is allowed to continue. <see langword="false"/> if otherwise;</returns>
        public virtual bool ProcessFile(FileInfo fileInfo)
        {
            // Only do something when the event has been declared.
            if (FileEvent != null)
            {
                RaiseFileEvent(fileInfo);
            }
            return true;
        }

        #endregion Overridable methods

        #region Private methods

        /// <summary>
        /// Walks the directory tree starting at the specified directory.
        /// </summary>
        /// <param name="directory"><see cref="DirectoryInfo"/> object for the current directory.</param>
        /// <returns><see langword="true"/> when the scan is allowed to continue. <see langword="false"/> if otherwise;</returns>
        private bool WalkDirectories(DirectoryInfo directory)
        {
            bool continueScan = true;

            if (continueScan = this.ProcessDirectory(directory, ScanDirectoryAction.Enter))
            {
                // Only scan the files in this path when a file event was specified
                if (this.FileEvent != null)
                {
                    continueScan = WalkFilesInDirectory(directory);
                }

                if (continueScan)
                {
                    DirectoryInfo[] subDirectories = directory.GetDirectories();

                    foreach (DirectoryInfo subDirectory in subDirectories)
                    {
                        // It is possible that users create a recursive directory by mounting a drive
                        // into an existing directory on that same drive. If so, the attributes
                        // will have the ReparsePoint flag active. The directory is then skipped.
                        // See: http://blogs.msdn.com/oldnewthing/archive/2004/12/27/332704.aspx
                        if ((subDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            continue;
                        }

                        if (!(continueScan = this.WalkDirectory(subDirectory)))
                        {
                            break;
                        }
                    }
                }

                if (continueScan)
                {
                    continueScan = this.ProcessDirectory(directory, ScanDirectoryAction.Leave);
                }
            }
            return continueScan;
        }

        /// <summary>
        /// Walks the directory tree starting at the specified path.
        /// </summary>
        /// <param name="directory"><see cref="DirectoryInfo"/> object for the current path.</param>
        /// <returns><see langword="true"/> when the scan was cancelled. <see langword="false"/> if otherwise;</returns>
        private bool WalkFilesInDirectory(DirectoryInfo directory)
        {
            bool continueScan = true;

            // Break up the search pattern in separate patterns
            string[] searchPatterns = _searchPattern.Split(';');

            // Try to find files for each search pattern
            foreach (string searchPattern in searchPatterns)
            {
                if (!continueScan)
                {
                    break;
                }
                // Scan all files in the current path
                foreach (FileInfo file in directory.GetFiles(searchPattern))
                {
                    if (!(continueScan = this.ProcessFile(file)))
                    {
                        break;
                    }
                }
            }
            return continueScan;
        }

        #endregion Private methods

        #region Properties

        private string _searchPattern;

        /// <summary>
        /// Gets or sets the search pattern.
        /// </summary>
        /// <example>
        /// You can specify more than one seach pattern
        /// </example>
        /// <value>The search pattern.</value>
        public string SearchPattern
        {
            get { return _searchPattern; }
            set
            {
                // When an empty value is specified, the search pattern will be the default (= *.*)
                if (value == null || value.Trim().Length == 0)
                {
                    _searchPattern = _patternAllFiles;
                }
                else
                {
                    _searchPattern = value;
                    // make sure the pattern does not end with a semi-colon
                    _searchPattern = _searchPattern.TrimEnd(new char[] { ';' });
                }
            }
        }

        #endregion Properties
    }
}