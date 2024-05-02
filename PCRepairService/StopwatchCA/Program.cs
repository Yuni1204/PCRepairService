using AsyncStopwatchCA;
using System.Linq.Expressions;

var DBContext = new CAStopDBContext();
var worker = new CAWorker(DBContext);

