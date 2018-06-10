using System;
using System.Collections.Generic;

namespace User.API.Models
{
    public class UserProperty
    {
        int? _requestedHashCode;

        public int AppUserId { get; set; }

        public string Key { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public override int GetHashCode()
        {
            //return (Key + Value).GetHashCode() ^ 31;
            if (!this.IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                {
                    _requestedHashCode = (Key + Value).GetHashCode() ^ 31;
                }

                return _requestedHashCode.Value;
            }
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is UserProperty))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            UserProperty item = (UserProperty)obj;

            if (item.IsTransient() || this.IsTransient())
            {
                return false;
            }
            else
            {
                return item.Key == this.Key && item.Value == this.Value;
            }
        }

        public bool IsTransient()
        {
            return string.IsNullOrEmpty(this.Key) || string.IsNullOrEmpty(this.Value);
        }

        public static bool operator ==(UserProperty left, UserProperty right)
        {
            if (Object.Equals(left, null))
            {
                return (Object.Equals(right, null)) ? true : false;
            }
            else
            {
                return left.Equals(right);
            }
        }
        public static bool operator !=(UserProperty left, UserProperty right)
        {
            return !(left == right);
        }

    }
}