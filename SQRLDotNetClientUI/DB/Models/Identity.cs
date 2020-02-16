﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SQRLDotNetClientUI.DB.Models
{
    /// <summary>
    /// This is a database model representing a SQRL identity record.
    /// </summary>
    public class Identity
    {
        /// <summary>
        /// The unique ID, generated by calculating the IDK
        /// for an empty domain and used as the primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The identity's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identity's raw data bytes.
        /// </summary>
        public byte[] DataBytes { get; set; }
    }
}
