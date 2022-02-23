using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
//using System.Data.DataSetExtensions;

using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace DatabaseWcfService
{
    [DataContract]
    public class EconomicFundamentals
    {
        private DateTime _releaseDateTime;
        private double _actual;
        private double _forecast;
        private double _previous;

        private string _eventHeadline;
        private string _eventType;

        private string _eventDescription;
        private string _category;
        private string _institutionBody;
        private string _currency;
        private string _country;
        private string _numericType;

        private string _importance;

        private string _associatedSymbolID;

        [DataMember]
        public DateTime ReleaseDateTime
        {
            get { return _releaseDateTime; }
            set { _releaseDateTime = value; }
        }

        [DataMember]
        public double Actual
        {
            get { return _actual; }
            set { _actual = value; }
        }

        [DataMember]
        public double Forecast
        {
            get { return _forecast; }
            set { _forecast = value; }
        }

        [DataMember]
        public double Previous
        {
            get { return _previous; }
            set { _previous = value; }
        }

        [DataMember]
        public string EventHeadline
        {
            get { return _eventHeadline; }
            set { _eventHeadline = value; }
        }

        [DataMember]
        public string EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        [DataMember]
        public string EventDescription
        {
            get { return _eventDescription; }
            set { _eventDescription = value; }
        }

        [DataMember]
        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        [DataMember]
        public string InstitutionBody
        {
            get { return _institutionBody; }
            set { _institutionBody = value; }
        }

        [DataMember]
        public string Currency
        {
            get { return _currency; }
            set { _currency = value; }
        }

        [DataMember]
        public string Country
        {
            get { return _country; }
            set { _country = value; }
        }

        [DataMember]
        public string NumericType
        {
            get { return _numericType; }
            set { _numericType = value; }
        }

        [DataMember]
        public string Importance
        {
            get { return _importance; }
            set { _importance = value; }
        }

        [DataMember]
        public string AssociatedSymbolID
        {
            get { return _associatedSymbolID; }
            set { _associatedSymbolID = value; }
        }
    }

    public struct EconomicFundamentalsEssentials
    {
        public string Event;
        public string Country;
        public string Category;
        public string Currency;

    }

}
