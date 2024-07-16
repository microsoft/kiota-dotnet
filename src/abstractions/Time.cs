// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Security;

namespace Microsoft.Kiota.Abstractions
{
    /// <summary>
    /// Model to represent only the date component of a DateTime
    /// </summary>
    public struct Time:IEquatable<Time>
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Converts the supplied <see cref="TimeOnly"/> parameter to <see cref="Time"/>.
        /// </summary>
        /// <param name="time">The <see cref="TimeOnly"/> to be converted.</param>
        /// <returns>A new <see cref="Time"/> structure whose hours, minutes, seconds and milliseconds are equal to those of the supplied time.</returns>
        public static implicit operator Time(TimeOnly time) => new(new DateTime(1, 1, 1, time.Hour, time.Minute, time.Second, time.Millisecond));

        /// <summary>
        /// Converts the supplied <see cref="Time"/> parameter to <see cref="TimeOnly"/>.
        /// </summary>
        /// <param name="time">The <see cref="Time"/> to be converted.</param>
        /// <returns>A new <see cref="TimeOnly"/> structure whose hours, minutes, seconds and milliseconds are equal to those of the supplied time.</returns>
        public static implicit operator TimeOnly(Time time) => new(time.DateTime.Hour, time.DateTime.Minute, time.DateTime.Second, time.DateTime.Millisecond);
#endif
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(Time other) => (Hour, Minute, Second) == (other.Hour, other.Minute, other.Second);

        /// <inheritdoc />
        public override bool Equals(object? o) => (o is Time other) && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(Hour, Minute, Second);
#else
            int hash = 17;
            hash = hash * 23 + Hour.GetHashCode();
            hash = hash * 23 + Minute.GetHashCode();
            hash = hash * 23 + Second.GetHashCode();
            return hash;
#endif
        }
        /// <summary>
        /// Create a new Time from hours, minutes, and seconds.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        public Time(int hour, int minute, int second)
            : this(new DateTime(1, 1, 1, hour, minute, second))
        {
        }

        /// <summary>
        /// Create a new Time from a <see cref="DateTime"/> object
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object to create the object from.</param>
        public Time(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        /// <summary>
        /// The <see cref="DateTime"/> representation of the class
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// The hour.
        /// </summary>
        public int Hour
        {
            get
            {
                return this.DateTime.Hour;
            }
        }

        /// <summary>
        /// The minute.
        /// </summary>
        public int Minute
        {
            get
            {
                return this.DateTime.Minute;
            }
        }

        /// <summary>
        /// The second.
        /// </summary>
        public int Second
        {
            get
            {
                return this.DateTime.Second;
            }
        }

        /// <summary>
        /// The time of day, formatted as "HH:mm:ss".
        /// </summary>
        /// <returns>The string time of day.</returns>
        public override string ToString()
        {
            return this.DateTime.ToString("HH\\:mm\\:ss");
        }
    }
}
