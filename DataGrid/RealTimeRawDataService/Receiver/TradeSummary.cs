using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Receiver
{

    [DataContract]
    public class TradeSummary
    {
        private DateTime _dateTime;
        private double _open;
        private double _high;
        private double _low;
        private double _close;
        private double _adjustmentclose;
        private int _volume;
        private String _timeFrame;
        private String _exchange;

        private String _symbolID;

        [DataMember]
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        [DataMember]
        public double Open
        {
            get { return _open; }
            set { _open = value; }
        }

        [DataMember]
        public double High
        {
            get { return _high; }
            set { _high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return _low; }
            set { _low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return _close; }
            set { _close = value; }
        }

        [DataMember]
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        [DataMember]
        public double AdjustmentClose
        {
            get { return _adjustmentclose; }
            set { _adjustmentclose = value; }
        }

        [DataMember]
        public String TimeFrame
        {
            get { return _timeFrame; }
            set { _timeFrame = value; }
        }

        [DataMember]
        public String SymbolID
        {
            get { return _symbolID; }
            set { _symbolID = value; }
        }

        [DataMember]
        public String Exchange
        {
            get { return _exchange; }
            set { _exchange = value; }
        }
    }
}
