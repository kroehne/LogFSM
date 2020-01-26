namespace LogDataTransformer_NEPS_V01
{
    #region usings
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using LogFSM_LogX2019;
    #endregion

    public static class TBAToolsLogReader
    {
        public static string XmlSerializeToString(this object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(this string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

       // private static JavaScriptSerializer jsonSerializer = new JavaScriptSerializer() { MaxJsonLength = 10000000 };

        public static List<logxGenericLogElement> getJSONLogData(string FileName, string PersonIdentifier)
        {
            if (!File.Exists(FileName))
                return new List<logxGenericLogElement>();

            try
            {
                List<TTLogElementWithReleativeTimestampOnly> _tmpList = new List<TTLogElementWithReleativeTimestampOnly>();
                StreamReader _r = new StreamReader(FileName);

                
                string jsonString = _r.ReadToEnd();
                //object[] _jsonArray = (object[])jsonSerializer.DeserializeObject(jsonString);
                object[] _jsonArray = (object[])JsonConvert.DeserializeObject(jsonString);

                if (null != _jsonArray)
                {
                    foreach (object[] _objectArray in _jsonArray)
                    {
                        int _messageCounter = int.Parse(_objectArray[0].ToString());
                        int _timeCounter = int.Parse(_objectArray[1].ToString());
                        string _key = _objectArray[2].ToString();
                        string _dataString = _objectArray[4].ToString();

                        if (_dataString.Replace("\"", "") == "START")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsTestStart).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });

                        }
                        else if (_dataString.Replace("\"", "") == "NEXT")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsNextItem).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else if (_dataString.Replace("\"", "") == "IGNORE_NEXT_WHILE_LOCKED")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsNextItemWhileLocked).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else if (_dataString.Replace("\"", "") == "RESTART")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsTestRestart).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });

                        }
                        else if (_dataString.Replace("\"", "") == "PREVIOUS")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsPreviousItem).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else if (_dataString.Replace("\"", "") == "END_OF_SEQUENCE")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsEndOfSequence).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else if (_dataString.Replace("\"", "") == "ITEM_NOT_FINISHED")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsItemNotFinished).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else if (_dataString.Replace("\"", "") == "LOGOUT")
                        {
                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                            {
                                RelativeTime = _messageCounter,
                                MessageCounter = _timeCounter,
                                logXElement = new logxGenericLogElement()
                                {
                                    Item = typeof(TBAToolsLog).Name,
                                    EventName = typeof(TBAToolsLogout).Name,
                                    EventDataXML = "",
                                    PersonIdentifier = PersonIdentifier,
                                    EventID = _timeCounter,
                                    RelativeTime = _messageCounter
                                }
                            });
                        }
                        else
                        {
                            //Dictionary<string, object> _dataObject = (Dictionary<string, object>)jsonSerializer.DeserializeObject(_objectArray[4].ToString());
                            Dictionary<string, object> _dataObject = (Dictionary<string, object>)JsonConvert.DeserializeObject(_objectArray[4].ToString());

                            if (_dataObject["type"].ToString() == "real_time")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = typeof(TBAToolsLog).Name,
                                        EventName = typeof(TBAToolsRealTime).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsRealTime() { RealTime = DateTime.Parse(_dataObject["data"].ToString()) }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "start_timestamp")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = typeof(TBAToolsLog).Name,
                                        EventName = typeof(TBAToolsLotStart).Name,
                                        EventDataXML = "",
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "loading")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsLoading).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsLoading() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "loaded")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsLoaded).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsLoaded() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "unloading")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsUnloading).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsUnloading() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "unloaded")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsUnloaded).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsUnloaded() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "user_interaction")
                            {
                                string _xml = _dataObject["data"].ToString();
                                XmlDocument _doc = new XmlDocument();
                                _doc.LoadXml(_xml);

                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = _doc.DocumentElement.LocalName,
                                        EventDataXML = _xml,
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "Received StopTask Event (Reading Items)")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsIBStopTask).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsIBStopTask() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "loaded again")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsIBLoadedAgain).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsIBLoadedAgain() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });

                            }
                            else if (_dataObject["type"].ToString() == "Received next task event from JBoss")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsIBReceivedNextTask).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsIBReceivedNextTask() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "Received StopTask Event")
                            {
                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                {
                                    RelativeTime = _messageCounter,
                                    MessageCounter = _timeCounter,
                                    logXElement = new logxGenericLogElement()
                                    {
                                        Item = _dataObject["sender"].ToString(),
                                        EventName = typeof(TBAToolsIBReceivedStopTask).Name,
                                        EventDataXML = XmlSerializeToString(new TBAToolsIBReceivedStopTask() { Sender = _dataObject["sender"].ToString() }),
                                        PersonIdentifier = PersonIdentifier,
                                        EventID = _timeCounter,
                                        RelativeTime = _messageCounter
                                    }
                                });
                            }
                            else if (_dataObject["type"].ToString() == "variable_change")
                            {
                                string _sender = _dataObject["sender"].ToString();
                                Dictionary<string, object> _data = (Dictionary<string, object>)_dataObject["data"];
                                if (!_data.ContainsKey("value"))
                                {
                                }
                                else
                                {
                                    if (_data["value"].GetType() == typeof(Dictionary<string, object>))
                                    {
                                        Dictionary<string, object> _values = (Dictionary<string, object>)_data["value"];
                                        foreach (string _k in _values.Keys)
                                        {
                                            string _variableName = _data["name"].ToString() + "_" + _k;
                                            if (_values[_k].ToString().Contains("ItemScore"))
                                            {
                                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                                {
                                                    RelativeTime = _messageCounter,
                                                    MessageCounter = _timeCounter,
                                                    logXElement = new logxGenericLogElement()
                                                    {
                                                        Item = _dataObject["sender"].ToString(),
                                                        EventName = "ItemScore",
                                                        EventDataXML = _values[_k].ToString(),
                                                        PersonIdentifier = PersonIdentifier,
                                                        EventID = _timeCounter,
                                                        RelativeTime = _messageCounter
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                                {
                                                    RelativeTime = _messageCounter,
                                                    MessageCounter = _timeCounter,
                                                    logXElement = new logxGenericLogElement()
                                                    {
                                                        Item = _dataObject["sender"].ToString(),
                                                        EventName = typeof(TBAToolsVariableChanged).Name,
                                                        EventDataXML = XmlSerializeToString(new TBAToolsVariableChanged() { Sender = _dataObject["sender"].ToString(), Variable = _variableName, Value = _values[_k].ToString() }),
                                                        PersonIdentifier = PersonIdentifier,
                                                        EventID = _timeCounter,
                                                        RelativeTime = _messageCounter
                                                    }
                                                });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string _variableName = _sender;
                                        if (_data["value"].ToString().Contains("ItemScore"))
                                        {
                                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                            {
                                                RelativeTime = _messageCounter,
                                                MessageCounter = _timeCounter,
                                                logXElement = new logxGenericLogElement()
                                                {
                                                    Item = _dataObject["sender"].ToString(),
                                                    EventName = "ItemScore",
                                                    EventDataXML = _data["value"].ToString(),
                                                    PersonIdentifier = PersonIdentifier,
                                                    EventID = _timeCounter,
                                                    RelativeTime = _messageCounter
                                                }
                                            });
                                        }
                                        else
                                        {
                                            _tmpList.Add(new TTLogElementWithReleativeTimestampOnly()
                                            {
                                                RelativeTime = _messageCounter,
                                                MessageCounter = _timeCounter,
                                                logXElement = new logxGenericLogElement()
                                                {
                                                    Item = _dataObject["sender"].ToString(),
                                                    EventName = typeof(TBAToolsVariableChanged).Name,
                                                    EventDataXML = XmlSerializeToString(new TBAToolsVariableChanged() { Sender = _dataObject["sender"].ToString(), Variable = _data["name"].ToString(), Value = _data["value"].ToString() }),
                                                    PersonIdentifier = PersonIdentifier,
                                                    EventID = _timeCounter,
                                                    RelativeTime = _messageCounter
                                                }
                                            });
                                        }
                                    }

                                }
                            }
                            else
                            {
                                Console.WriteLine(_dataObject["type"].ToString());
                            }
                        }

                    }

                    // Sort by message counter

                    _tmpList.Sort(delegate (TTLogElementWithReleativeTimestampOnly l1, TTLogElementWithReleativeTimestampOnly l2)
                    {
                        return l1.MessageCounter.CompareTo(l2.MessageCounter);
                    });


                    // Update absolute times

                    DateTime _lastStartTime = DateTime.MinValue;
                    long _offset = 0;
                    long _lastOffset = 0;
                    for (int i = 0; i < _tmpList.Count; i++)
                    {
                        if (_tmpList[i].logXElement.EventName == "TBAToolsRealTime")
                        {
                            _lastStartTime = TBAToolsLogReader.XmlDeserializeFromString<TBAToolsRealTime>(_tmpList[i].logXElement.EventDataXML).RealTime;
                            _offset = _lastOffset;
                        }
                        else
                        {
                            _lastOffset = _tmpList[i].RelativeTime;
                        }
                        if (_lastStartTime != DateTime.MinValue)
                        {
                            _tmpList[i].logXElement.TimeStamp = _lastStartTime + TimeSpan.FromMilliseconds(_tmpList[i].RelativeTime - _offset);
                            _tmpList[i].AbsoluteTime = _tmpList[i].logXElement.TimeStamp;
                        }

                        _tmpList[i].Offset = _offset;
                    }

                    DateTime _lastTime = DateTime.MinValue;
                    for (int i = _tmpList.Count - 1; i >= 0; i--)
                    {
                        if (_tmpList[i].AbsoluteTime != DateTime.MinValue)
                        {
                            _lastTime = _tmpList[i].AbsoluteTime;
                        }
                        if (_tmpList[i].AbsoluteTime == DateTime.MinValue && _lastTime != DateTime.MinValue)
                        {
                            _tmpList[i].logXElement.TimeStamp = _lastTime - TimeSpan.FromMilliseconds(_tmpList[i].RelativeTime - _tmpList[i].Offset);
                        }
                    }
                }

                List<logxGenericLogElement> _tmpReturn = new List<logxGenericLogElement>();
                foreach (var v in _tmpList)
                    _tmpReturn.Add(v.logXElement);

                return _tmpReturn;
            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.ToString());
            }

            return new List<logxGenericLogElement>();
        }

    }

    public class TTLogElementWithReleativeTimestampOnly
    {
        public long RelativeTime { get; set; }
        public long MessageCounter { get; set; }
        public logxGenericLogElement logXElement { get; set; }
        public long Offset { get; set; }
        public DateTime AbsoluteTime { get; set; }
    }


}
