using QSP.AviationTools.Coordinates;
using QSP.RouteFinding.AirwayStructure;
using QSP.RouteFinding.RouteAnalyzers;
using QSP.RouteFinding.Routes;
using QSP.RouteFinding.Airports;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace QSP.RouteFinding.Tracks.Common
{
    // Read the track waypoints as strings, and try to find each waypoints in WptList
    public class TrackReader<T> where T : Track
    {
        private WaypointList wptList;
        private AirportManager airportList;

        private List<WptPair> routeFromTo;
        private Route mainRoute;
        private T trk;

        public TrackReader(WaypointList wptList, AirportManager airportList)
        {
            this.wptList = wptList;
            this.airportList = airportList;
        }

        /// <exception cref="InvalidRouteException"></exception>
        /// <exception cref="WaypointNotFoundException"></exception>
        public TrackNodes Read(T item)
        {
            trk = item;
            mainRoute = readMainRoute(trk.MainRoute);

            // The format of this part is rather unpredictable. 
            // For example, a route can even start with an airway:
            // RTS/CYVR V317 QQ YZT JOWEN 
            // ...
            // Since this part is not that important, we can allow it to fail and still ignore it.
            try
            {
                routeFromTo = findWptAllRouteFrom(trk.RouteFrom);
                routeFromTo.AddRange(findWptAllRouteTo(trk.RouteTo));
                routeFromTo = routeFromTo.Distinct().ToList();
            }
            catch
            {
                routeFromTo = new List<WptPair>();
            }

            return new TrackNodes(trk.Ident, trk.AirwayIdent, mainRoute, routeFromTo);
        }

        #region "Method for routeFrom/To"

        private List<WptPair> getExtraPairs(string[] rteFrom, double prevLat, double prevLon)
        {
            var result = new List<WptPair>();
            int lastIndex = -1;

            for (int index = 0; index < rteFrom.Length; index++)
            {
                if (lastIndex >= 0)
                {
                    if (isAirway(lastIndex, rteFrom[index]))
                    {
                        lastIndex = -1;
                    }
                    else
                    {
                        int wpt = selectWpt(prevLat, prevLon, rteFrom[index]);
                        result.Add(new WptPair(lastIndex, wpt));
                        lastIndex = wpt;
                    }
                }
                else
                {
                    lastIndex = selectWpt(prevLat, prevLon, rteFrom[index]);
                }
            }
            return result;
        }

        private int selectWpt(double prevLat, double prevLon, string ident)
        {
            var candidates = wptList.FindAllByID(ident);

            if (candidates == null || candidates.Count == 0)
            {
                throw new TrackWaypointNotFoundException("Waypoint not found.");
            }
            return Utilities.ChooseSubsequentWpt(prevLat, prevLon, candidates, wptList);
        }

        private bool isAirway(int lastIndex, string airway)
        {
            if (airway == "UPR")
            {
                return true;
            }

            foreach (var i in wptList.EdgesFrom(lastIndex))
            {
                if (wptList.GetEdge(i).Value.Airway == airway)
                {
                    return true;
                }
            }
            return false;
        }

        private List<WptPair> findWptAllRouteFrom(ReadOnlyCollection<string[]> rteFrom)
        {
            var result = new List<WptPair>();
            var firstWpt = mainRoute.First.Waypoint;

            foreach (var i in rteFrom)
            {
                result.AddRange(getExtraPairs(i, firstWpt.Lat, firstWpt.Lon));
            }

            return result;
        }

        private List<WptPair> findWptAllRouteTo(ReadOnlyCollection<string[]> rteTo)
        {
            var result = new List<WptPair>();
            var lastWpt = mainRoute.Last.Waypoint;

            foreach (var i in rteTo)
            {
                result.AddRange(getExtraPairs(i, lastWpt.Lat, lastWpt.Lon));
            }

            return result;
        }

        #endregion

        #region "Method for main route"

        /// <exception cref="InvalidRouteException"></exception>
        /// <exception cref="WaypointNotFoundException"></exception>
        private Route readMainRoute(ReadOnlyCollection<string> rte)
        {
            LatLon latLon = trk.PreferredFirstLatLon;
            return new AutoSelectAnalyzer(new CoordinateFormatter(
                                                 combineArray(rte)).Split(),
                                                 latLon.Lat,
                                                 latLon.Lon,
                                                 wptList)
                                          .Analyze();
        }

        private string combineArray(ReadOnlyCollection<string> item)
        {
            var result = new StringBuilder();

            for (int i = 0; i < item.Count; i++)
            {
                string s = item[i];

                if ((i != 0 && i != item.Count - 1) ||
                    airportList.Find(s) == null)
                {
                    result.Append(s + " ");
                }
            }
            return result.ToString();
        }

        #endregion

    }
}