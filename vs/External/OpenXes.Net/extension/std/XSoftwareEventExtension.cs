using System;
using OpenXesNet.model;
using OpenXesNet.info;
using OpenXesNet.factory;
using System.Collections.Generic;

namespace OpenXesNet.extension.std
{
    /// <summary>
    /// During  the  execution  of  software,  execution  data  can  be  recorded.
    /// With  the development of  process mining  techniques on  the one  hand,  
    /// and the  growing availability of software execution data on the other hand, 
    /// a new form of software analytics  comes into  reach.That  is,  applying 
    /// process  mining techniques  to analyze software execution data. To enable 
    /// process mining for software, event logs should be capable of capturing 
    /// software-specific data.
    /// 
    /// A  software  event  log  is  typically recorded  at the  method call  level 
    /// during software execution.  Events generated at this level reference a 
    /// specific point in the software  source code.   The Software  Event extension  
    /// captures  this  event location information, together with some basic runtime 
    /// information related to this location.
    /// </summary>
    public class XSoftwareEventExtension : XExtension
    {
        public static readonly Uri EXTENSION_URI = new UriBuilder("http://www.xes-standard.org/swevent.xesext").Uri;

        #region Keys definition
        static readonly string KEY_APP_NAME = "appName";
        static readonly string KEY_APP_NODE = "appNode";
        static readonly string KEY_APP_SESSION = "appSession";
        static readonly string KEY_APP_TIER = "appTier";
        static readonly string KEY_CALLEE_CLASS = "callee-class";
        static readonly string KEY_CALLEE_FILENAME = "callee-filename";
        static readonly string KEY_CALLEE_INSTANCEID = "callee-instanceId";
        static readonly string KEY_CALLEE_ISCONSTRUCTOR = "callee-isConstructor";
        static readonly string KEY_CALLEE_LINENR = "callee-lineNr";
        static readonly string KEY_CALLEE_METHOD = "callee-method";
        static readonly string KEY_CALLEE_PACKAGE = "callee-package";
        static readonly string KEY_CALLEE_PARAMSIG = "callee-paramSig";
        static readonly string KEY_CALLEE_RETURNSIG = "callee-returnSig";
        static readonly string KEY_CALLER_CLASS = "caller-class";
        static readonly string KEY_CALLER_FILENAME = "caller-filename";
        static readonly string KEY_CALLER_INSTANCEID = "caller-instanceId";
        static readonly string KEY_CALLER_ISCONSTRUCTOR = "caller-isConstructor";
        static readonly string KEY_CALLER_LINENR = "caller-lineNr";
        static readonly string KEY_CALLER_METHOD = "caller-method";
        static readonly string KEY_CALLER_PACKAGE = "caller-package";
        static readonly string KEY_CALLER_PARAMSIG = "caller-paramSig";
        static readonly string KEY_CALLER_RETURNSIG = "caller-returnSig";
        static readonly string KEY_EX_CAUGHT = "exCaught";
        static readonly string KEY_EX_THROWN = "exThrown";
        static readonly string KEY_HAS_DATA = "hasData";
        static readonly string KEY_HAS_EXCEPTION = "hasException";
        static readonly string KEY_NANOTIME = "nanotime";
        static readonly string KEY_PARAMS = "params";
        static readonly string KEY_PARAM_VALUE = "paramValue";
        static readonly string KEY_RETURN_VALUE = "returnValue";
        static readonly string KEY_THREAD_ID = "caller-threadId";
        static readonly string KEY_TYPE = "type";
        static readonly string KEY_VALUE_TYPE = "valueType";
        #endregion

        #region Attribute definition

        #region Event Type attributes
        /// <summary>
        /// Software event type, indicating at what point during  execution this  
        /// event was generated.  The possible values are:
        /// <list type="bullet">
        ///     <item>
        ///         <term>call</term>
        ///         <description>The start of a method block.</description>
        ///     </item>
        ///     <item>
        ///         <term>return</term>
        ///         <description>The normal end of a method block</description>
        ///     </item>
        ///     <item>
        ///         <term>throws</term>
        ///         <description>The end of a method block in case of an uncaught exception</description>
        ///     </item>
        ///     <item>
        ///         <term>handle</term>
        ///         <description>The start of an exception handle catch block</description>
        ///     </item>
        ///     <item>
        ///         <term>calling</term>
        ///         <description>The start of calling / invoking another method</description>
        ///     </item>
        ///     <item>
        ///         <term>returning</term>
        ///         <description>The end of returning a called method</description>
        ///     </item>
        /// </list>
        /// </summary>
        public static XAttributeLiteral ATTR_TYPE;
        #endregion

        #region App attributes
        /// <summary>
        /// The user defined application name
        /// </summary>
        public static XAttributeLiteral ATTR_APP_NAME;
        /// <summary>
        /// The user defined application node
        /// </summary>
        public static XAttributeLiteral ATTR_APP_NODE;
        /// <summary>
        /// The user defined application session
        /// </summary>
        public static XAttributeLiteral ATTR_APP_SESSION;
        /// <summary>
        /// The user defined application tier
        /// </summary>
        public static XAttributeLiteral ATTR_APP_TIER;
        #endregion

        #region Event Location attributes

        #region Callee
        /// <summary>
        /// The class to which the calee method belongs.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_CLASS;
        /// <summary>
        /// The file name of the callee source code artifact
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_FILENAME;
        /// <summary>
        /// The instance id of the callee class instance. The absence of an 
        /// instance id is represented by the value "0"
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_INSTANCEID;
        /// <summary>
        /// If the rerefenced method in the callee class is a class constructor
        /// </summary>
        public static XAttributeBoolean ATTR_CALLEE_ISCONSTRUCTOR;
        /// <summary>
        /// The line number of the callee executed source code statement
        /// </summary>
        public static XAttributeDiscrete ATTR_CALLEE_LINENR;
        /// <summary>
        /// The callee's referenced method.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_METHOD;
        /// <summary>
        /// The package in the software code architecture to which the callee's method belongs.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_PACKAGE;
        /// <summary>
        /// The parameter signature of the callee's referenced method
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_PARAMSIG;
        /// <summary>
        /// The return  signature of the callee's method
        /// </summary>
        public static XAttributeLiteral ATTR_CALLEE_RETURNSIG;
        #endregion

        #region Caller
        /// <summary>
        /// The class to which the caler method belongs.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_CLASS;
        /// <summary>
        /// The file name of the caller source code artifact
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_FILENAME;
        /// <summary>
        /// The instance id of the caller class instance. The absence of an 
        /// instance id is represented by the value "0"
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_INSTANCEID;
        /// <summary>
        /// If the rerefenced method in the caller class is a class constructor
        /// </summary>
        public static IXAttributeBoolean ATTR_CALLER_ISCONSTRUCTOR;
        /// <summary>
        /// The line number of the caller executed source code statement
        /// </summary>
        public static IXAttributeDiscrete ATTR_CALLER_LINENR;
        /// <summary>
        /// The caller's referenced method.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_METHOD;
        /// <summary>
        /// The package in the software code architecture to which the caller's method belongs.
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_PACKAGE;
        /// <summary>
        /// The parameter signature of the caller's referenced method
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_PARAMSIG;
        /// <summary>
        /// The return  signature of the caller's method
        /// </summary>
        public static XAttributeLiteral ATTR_CALLER_RETURNSIG;
        #endregion
        #endregion

        #region Exception attributes
        /// <summary>
        /// If exception data is recorded for this log
        /// </summary>
        public static IXAttributeBoolean ATTR_HAS_EXCEPTION;
        /// <summary>
        /// The caught exception type for a handle event
        /// </summary>
        public static XAttributeLiteral ATTR_EX_CAUGHT;
        /// <summary>
        /// The thrown exception type for a throws or handle event
        /// </summary>
        public static XAttributeLiteral ATTR_EX_THROWN;
        #endregion

        #region Method attributes
        /// <summary>
        /// If method data is recorded for this log
        /// </summary>
        public static IXAttributeBoolean ATTR_HAS_DATA;
        /// <summary>
        /// The return value for the returning method
        /// </summary>
        public static XAttributeLiteral ATTR_RETURN_VALUE;
        /// <summary>
        /// List of parameters for the called method
        /// </summary>
        public static IXAttributeList ATTR_PARAMS;
        /// <summary>
        /// A parameter value in the list of parameters
        /// </summary>
        public static XAttributeLiteral ATTR_PARAM_VALUE;
        /// <summary>
        /// The runtime value type for a return or return parameter value
        /// </summary>
        public static XAttributeLiteral ATTR_VALUE_TYPE;
        #endregion

        #region Runtime attributes
        /// <summary>
        /// The elapsed nano time since some fixed but arbitrary time.
        /// </summary>
        public static IXAttributeDiscrete ATTR_NANOTIME;
        /// <summary>
        /// The thread id on which the event was generated
        /// </summary>
        public static XAttributeLiteral ATTR_THREAD_ID;
        #endregion

        #endregion

        static XSoftwareEventExtension singleton = new XSoftwareEventExtension();


        public enum SoftwareEventType
        {
            CALL,
            CALLING,
            HANDLE,
            RETURN,
            RETURNING,
            THROWS,
            UNKNOWN
        }

        public static XSoftwareEventExtension Instance
        {
            get { return singleton; }
        }

        XSoftwareEventExtension() : base("Software  evt", "swevent", EXTENSION_URI)
        {
            IXFactory factory = XFactoryRegistry.Instance.CurrentDefault;

            #region Application Information
            ATTR_APP_NAME = factory.CreateAttributeLiteral(KEY_APP_NAME, "__INVALID__", this);
            this.eventAttributes.Add(KEY_APP_NAME, (XAttribute)ATTR_APP_NAME.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_APP_NAME), "User defined application name");

            ATTR_APP_NODE = factory.CreateAttributeLiteral(KEY_APP_NODE, "__INVALID__", this);
            this.eventAttributes.Add(KEY_APP_NODE, (XAttribute)ATTR_APP_NODE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_APP_NODE), "User defined application node");

            ATTR_APP_SESSION = factory.CreateAttributeLiteral(KEY_APP_SESSION, "__INVALID__", this);
            this.eventAttributes.Add(KEY_APP_SESSION, (XAttribute)ATTR_APP_SESSION.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_APP_SESSION), "User defined application session");

            ATTR_APP_TIER = factory.CreateAttributeLiteral(KEY_APP_TIER, "__INVALID__", this);
            this.eventAttributes.Add(KEY_APP_TIER, (XAttribute)ATTR_APP_TIER.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_APP_TIER), "User defined application tier");
            #endregion

            #region Callee event location
            ATTR_CALLEE_PACKAGE = factory.CreateAttributeLiteral(KEY_CALLEE_PACKAGE, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_PACKAGE, (XAttribute)ATTR_CALLEE_PACKAGE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_PACKAGE), "Callee - Package");

            ATTR_CALLEE_CLASS = factory.CreateAttributeLiteral(KEY_CALLEE_CLASS, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_CLASS, (XAttribute)ATTR_CALLEE_CLASS.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_CLASS), "Callee - Class");

            ATTR_CALLEE_METHOD = factory.CreateAttributeLiteral(KEY_CALLEE_METHOD, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_METHOD, (XAttribute)ATTR_CALLEE_METHOD.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_METHOD), "Callee - Method");

            ATTR_CALLEE_PARAMSIG = factory.CreateAttributeLiteral(KEY_CALLEE_PARAMSIG, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_PARAMSIG, (XAttribute)ATTR_CALLEE_PARAMSIG.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_PARAMSIG), "Callee - Parameter signature");

            ATTR_CALLEE_RETURNSIG = factory.CreateAttributeLiteral(KEY_CALLEE_RETURNSIG, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_RETURNSIG, (XAttribute)ATTR_CALLEE_RETURNSIG.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_RETURNSIG), "Callee - Return signature");

            ATTR_CALLEE_ISCONSTRUCTOR = factory.CreateAttributeBoolean(KEY_CALLEE_ISCONSTRUCTOR, false, this);
            this.eventAttributes.Add(KEY_CALLEE_ISCONSTRUCTOR, (XAttribute)ATTR_CALLEE_ISCONSTRUCTOR.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_ISCONSTRUCTOR), "Callee - Is a class constructor");

            ATTR_CALLEE_INSTANCEID = factory.CreateAttributeLiteral(KEY_CALLEE_INSTANCEID, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_INSTANCEID, (XAttribute)ATTR_CALLEE_INSTANCEID.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_INSTANCEID), "Callee - Instance id of class instance");

            ATTR_CALLEE_FILENAME = factory.CreateAttributeLiteral(KEY_CALLEE_FILENAME, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLEE_FILENAME, (XAttribute)ATTR_CALLEE_FILENAME.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_FILENAME), "Callee - File name source code artifact");

            ATTR_CALLEE_LINENR = factory.CreateAttributeDiscrete(KEY_CALLEE_LINENR, -1L, this);
            this.eventAttributes.Add(KEY_CALLEE_LINENR, (XAttribute)ATTR_CALLEE_LINENR.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLEE_LINENR), "Callee - Line number is source code artifact");
            #endregion

            #region Caller event Location
            ATTR_CALLER_PACKAGE = factory.CreateAttributeLiteral(KEY_CALLER_PACKAGE, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_PACKAGE, (XAttribute)ATTR_CALLER_PACKAGE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_PACKAGE), "Caller - Package");

            ATTR_CALLER_CLASS = factory.CreateAttributeLiteral(KEY_CALLER_CLASS, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_CLASS, (XAttribute)ATTR_CALLER_CLASS.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_CLASS), "Caller - Class");

            ATTR_CALLER_METHOD = factory.CreateAttributeLiteral(KEY_CALLER_METHOD, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_METHOD, (XAttribute)ATTR_CALLER_METHOD.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_METHOD), "Caller - Method");

            ATTR_CALLER_PARAMSIG = factory.CreateAttributeLiteral(KEY_CALLER_PARAMSIG, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_PARAMSIG, (XAttribute)ATTR_CALLER_PARAMSIG.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_PARAMSIG), "Caller - Parameter signature");

            ATTR_CALLER_RETURNSIG = factory.CreateAttributeLiteral(KEY_CALLER_RETURNSIG, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_RETURNSIG, (XAttribute)ATTR_CALLER_RETURNSIG.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_RETURNSIG), "Caller - Return signature");

            ATTR_CALLER_ISCONSTRUCTOR = factory.CreateAttributeBoolean(KEY_CALLER_ISCONSTRUCTOR, false, this);
            this.eventAttributes.Add(KEY_CALLER_ISCONSTRUCTOR, (XAttribute)ATTR_CALLER_ISCONSTRUCTOR.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_ISCONSTRUCTOR), "Caller - Is a class constructor");

            ATTR_CALLER_INSTANCEID = factory.CreateAttributeLiteral(KEY_CALLER_INSTANCEID, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_INSTANCEID, (XAttribute)ATTR_CALLER_INSTANCEID.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_INSTANCEID), "Caller - Instance id of class instance");

            ATTR_CALLER_FILENAME = factory.CreateAttributeLiteral(KEY_CALLER_FILENAME, "__INVALID__", this);
            this.eventAttributes.Add(KEY_CALLER_FILENAME, (XAttribute)ATTR_CALLER_FILENAME.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_FILENAME), "Caller - File name source code artifact");

            ATTR_CALLER_LINENR = factory.CreateAttributeDiscrete(KEY_CALLER_LINENR, -1L, this);
            this.eventAttributes.Add(KEY_CALLER_LINENR, (XAttribute)ATTR_CALLER_LINENR.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_CALLER_LINENR), "Caller - Line number is source code artifact");
            #endregion

            #region Exception Information
            ATTR_HAS_EXCEPTION = factory.CreateAttributeBoolean(KEY_HAS_EXCEPTION, false, this);
            this.logAttributes.Add(KEY_HAS_EXCEPTION, (XAttribute)ATTR_HAS_EXCEPTION.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_HAS_EXCEPTION), "Has exception data");

            ATTR_EX_CAUGHT = factory.CreateAttributeLiteral(KEY_EX_CAUGHT, "__INVALID__", this);
            this.eventAttributes.Add(KEY_EX_CAUGHT, (XAttribute)ATTR_EX_CAUGHT.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_EX_CAUGHT), "Caught exception type");

            ATTR_EX_THROWN = factory.CreateAttributeLiteral(KEY_EX_THROWN, "__INVALID__", this);
            this.eventAttributes.Add(KEY_EX_THROWN, (XAttribute)ATTR_EX_THROWN.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_EX_THROWN), "Thrown exception type");
            #endregion

            #region Runtime Information
            ATTR_NANOTIME = factory.CreateAttributeDiscrete(KEY_NANOTIME, -1L, this);
            this.eventAttributes.Add(KEY_NANOTIME, (XAttribute)ATTR_NANOTIME.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_NANOTIME), "Elapsed nanotime");

            ATTR_THREAD_ID = factory.CreateAttributeLiteral(KEY_THREAD_ID, "__INVALID__", this);
            this.eventAttributes.Add(KEY_THREAD_ID, (XAttribute)ATTR_THREAD_ID.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_THREAD_ID), "Thread id for generated event");
            #endregion

            #region Method Data
            ATTR_HAS_DATA = factory.CreateAttributeBoolean(KEY_HAS_DATA, false, this);
            this.logAttributes.Add(KEY_HAS_DATA, (XAttribute)ATTR_HAS_DATA.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_HAS_DATA), "Has method data");

            ATTR_RETURN_VALUE = factory.CreateAttributeLiteral(KEY_RETURN_VALUE, "__INVALID__", this);
            this.eventAttributes.Add(KEY_RETURN_VALUE, (XAttribute)ATTR_RETURN_VALUE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_RETURN_VALUE), "Return value for the returning method");

            ATTR_PARAMS = factory.CreateAttributeList(KEY_PARAMS, this);
            this.eventAttributes.Add(KEY_PARAMS, (XAttribute)ATTR_PARAMS.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_PARAMS), "List of parameters for the called method");

            ATTR_PARAM_VALUE = factory.CreateAttributeLiteral(KEY_PARAM_VALUE, "__INVALID__", this);
            this.metaAttributes.Add(KEY_PARAM_VALUE, (XAttribute)ATTR_PARAM_VALUE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_PARAM_VALUE), "A parameter value in the list params");

            ATTR_VALUE_TYPE = factory.CreateAttributeLiteral(KEY_VALUE_TYPE, "__INVALID__", this);
            this.metaAttributes.Add(KEY_VALUE_TYPE, (XAttribute)ATTR_VALUE_TYPE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_VALUE_TYPE), "A runtime value type for a return or parameter value");
            #endregion

            #region Event type and lifecycle
            ATTR_TYPE = factory.CreateAttributeLiteral(KEY_TYPE, "__INVALID__", this);
            this.eventAttributes.Add(KEY_TYPE, (XAttribute)ATTR_TYPE.Clone());
            XGlobalAttributeNameMap.Instance.RegisterMapping("EN", QualifiedName(KEY_TYPE), "Event type");
            #endregion
        }

        /// <summary>
        /// Extracts the name "swevent:appName" value from a specific event.
        /// </summary>
        /// <returns>A string with attribute value if defined, <code>null</code> otherwise.</returns>
        /// <param name="evt">The XEvent to search for the attribute.</param>
        public string ExtractAppName(XEvent evt)
        {
            return Extract(evt, KEY_APP_NAME, (string)null);
        }

        public XAttributeLiteral AssignAppName(XEvent evt, string appName)
        {
            return Assign(evt, ATTR_APP_NAME, appName);
        }

        public bool RemoveAppName(XEvent evt)
        {
            return Remove(evt, KEY_APP_NAME);
        }

        public string ExtractAppNode(XEvent evt)
        {
            return Extract(evt, KEY_APP_NODE, (string)null);
        }

        public XAttributeLiteral AssignAppNode(XEvent evt, string appNode)
        {
            return Assign(evt, ATTR_APP_NODE, appNode);
        }

        public bool RemoveAppNode(XEvent evt)
        {
            return Remove(evt, KEY_APP_NODE);
        }

        public string ExtractAppSession(XEvent evt)
        {
            return Extract(evt, KEY_APP_SESSION, (string)null);
        }

        public XAttributeLiteral AssignAppSession(XEvent evt, string appSession)
        {
            return Assign(evt, ATTR_APP_SESSION, appSession);
        }

        public bool RemoveAppSession(XEvent evt)
        {
            return Remove(evt, KEY_APP_SESSION);
        }

        public string ExtractAppTier(XEvent evt)
        {
            return Extract(evt, KEY_APP_TIER, (string)null);
        }

        public XAttributeLiteral AssignAppTier(XEvent evt, string appTier)
        {
            return Assign(evt, ATTR_APP_TIER, appTier);
        }

        public bool RemoveAppTier(XEvent evt)
        {
            return Remove(evt, KEY_APP_TIER);
        }

        public string ExtractCalleeClass(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_CLASS, (string)null);
        }

        public XAttributeLiteral AssignCalleeClass(XEvent evt, string calleeClass)
        {
            return Assign(evt, ATTR_CALLEE_CLASS, calleeClass);
        }

        public bool RemoveCalleeClass(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_CLASS);
        }

        public string ExtractCalleeFilename(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_FILENAME, (string)null);
        }

        public XAttributeLiteral AssignCalleeFilename(XEvent evt, string calleeFilename)
        {
            return Assign(evt, ATTR_CALLEE_FILENAME, calleeFilename);
        }

        public bool RemoveCalleeFilename(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_FILENAME);
        }

        public string ExtractCalleeInstanceId(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_INSTANCEID, (string)null);
        }

        public XAttributeLiteral AssignCalleeInstanceId(XEvent evt, string calleeInstanceId)
        {
            return Assign(evt, ATTR_CALLEE_INSTANCEID, calleeInstanceId);
        }

        public bool RemoveCalleeInstanceId(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_INSTANCEID);
        }

        public bool ExtractCalleeIsConstructor(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_ISCONSTRUCTOR, false);
        }

        public IXAttributeBoolean AssignCalleeIsConstructor(XEvent evt, bool calleeInstanceId)
        {
            return Assign(evt, ATTR_CALLEE_ISCONSTRUCTOR, calleeInstanceId);
        }

        public bool RemoveCalleeIsConstructor(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_ISCONSTRUCTOR);
        }

        public long ExtractCalleeLineNr(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_LINENR, -1L);
        }

        public IXAttributeDiscrete AssignCalleeLineNr(XEvent evt, long calleeLineNr)
        {
            return Assign(evt, ATTR_CALLEE_LINENR, calleeLineNr);
        }

        public bool RemoveCalleeLineNr(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_LINENR);
        }

        public string ExtractCalleeMethod(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_METHOD, (string)null);
        }

        public XAttributeLiteral AssignCalleeMethod(XEvent evt, string calleeMethod)
        {
            return Assign(evt, ATTR_CALLEE_METHOD, calleeMethod);
        }

        public bool RemoveCalleeMethod(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_METHOD);
        }

        public string ExtractCalleePackage(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_PACKAGE, (string)null);
        }

        public XAttributeLiteral AssignCalleePackage(XEvent evt, string calleePackage)
        {
            return Assign(evt, ATTR_CALLEE_PACKAGE, calleePackage);
        }

        public bool RemoveCalleePackage(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_PACKAGE);
        }

        public string ExtractCalleeParamSig(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_PARAMSIG, (string)null);
        }

        public XAttributeLiteral AssignCalleeParamSig(XEvent evt, string calleeParamSig)
        {
            return Assign(evt, ATTR_CALLEE_PARAMSIG, calleeParamSig);
        }

        public bool RemoveCalleeParamSig(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_PARAMSIG);
        }

        public string ExtractCalleeReturnSig(XEvent evt)
        {
            return Extract(evt, KEY_CALLEE_RETURNSIG, (string)null);
        }

        public XAttributeLiteral AssignCalleeReturnSig(XEvent evt, string calleeReturnSig)
        {
            return Assign(evt, ATTR_CALLEE_RETURNSIG, calleeReturnSig);
        }

        public bool RemoveCalleeReturnSig(XEvent evt)
        {
            return Remove(evt, KEY_CALLEE_RETURNSIG);
        }

        public string ExtractCallerClass(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_CLASS, (string)null);
        }

        public XAttributeLiteral AssignCallerClass(XEvent evt, string callerClass)
        {
            return Assign(evt, ATTR_CALLER_CLASS, callerClass);
        }

        public bool RemoveCallerClass(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_CLASS);
        }

        public string ExtractCallerFilename(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_FILENAME, (string)null);
        }

        public XAttributeLiteral AssignCallerFilename(XEvent evt, string callerFilename)
        {
            return Assign(evt, ATTR_CALLER_FILENAME, callerFilename);
        }

        public bool RemoveCallerFilename(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_FILENAME);
        }

        public string ExtractCallerInstanceId(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_INSTANCEID, (string)null);
        }

        public XAttributeLiteral AssignCallerInstanceId(XEvent evt, string callerInstanceId)
        {
            return Assign(evt, ATTR_CALLER_INSTANCEID, callerInstanceId);
        }

        public bool RemoveCallerInstanceId(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_INSTANCEID);
        }

        public bool ExtractCallerIsConstructor(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_ISCONSTRUCTOR, false);
        }

        public IXAttributeBoolean AssignCallerIsConstructor(XEvent evt, bool callerInstanceId)
        {
            return Assign(evt, ATTR_CALLER_ISCONSTRUCTOR, callerInstanceId);
        }

        public bool RemoveCallerIsConstructor(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_ISCONSTRUCTOR);
        }

        public long ExtractCallerLineNr(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_LINENR, -1L);
        }

        public IXAttributeDiscrete AssignCallerLineNr(XEvent evt, long callerLineNr)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_CALLER_LINENR, callerLineNr);
        }

        public bool RemoveCallerLineNr(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_LINENR);
        }

        public string ExtractCallerMethod(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_METHOD, (string)null);
        }

        public XAttributeLiteral AssignCallerMethod(XEvent evt, string callerMethod)
        {
            return Assign(evt, ATTR_CALLER_METHOD, callerMethod);
        }

        public bool RemoveCallerMethod(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_METHOD);
        }

        public string ExtractCallerPackage(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_PACKAGE, (string)null);
        }

        public XAttributeLiteral AssignCallerPackage(XEvent evt, string callerPackage)
        {
            return Assign(evt, ATTR_CALLER_PACKAGE, callerPackage);
        }

        public bool RemoveCallerPackage(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_PACKAGE);
        }

        public string ExtractCallerParamSig(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_PARAMSIG, (string)null);
        }

        public XAttributeLiteral AssignCallerParamSig(XEvent evt, string callerParamSig)
        {
            return Assign(evt, ATTR_CALLER_PARAMSIG, callerParamSig);
        }

        public bool RemoveCallerParamSig(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_PARAMSIG);
        }

        public string ExtractCallerReturnSig(XEvent evt)
        {
            return Extract(evt, KEY_CALLER_RETURNSIG, (string)null);
        }

        public XAttributeLiteral AssignCallerReturnSig(XEvent evt, string callerReturnSig)
        {
            return Assign(evt, ATTR_CALLER_RETURNSIG, callerReturnSig);
        }

        public bool RemoveCallerReturnSig(XEvent evt)
        {
            return Remove(evt, KEY_CALLER_RETURNSIG);
        }

        public string ExtractExCaught(XEvent evt)
        {
            return Extract(evt, KEY_EX_CAUGHT, (string)null);
        }

        public XAttributeLiteral AssignExCaught(XEvent evt, string exCaught)
        {
            return Assign(evt, ATTR_EX_CAUGHT, exCaught);
        }
        public bool RemoveExCaught(XEvent evt)
        {
            return Remove(evt, KEY_EX_CAUGHT);
        }

        public string ExtractExThrown(XEvent evt)
        {
            return Extract(evt, KEY_EX_THROWN, (string)null);
        }

        public XAttributeLiteral AssignExThrown(XEvent evt, string exThrown)
        {
            return Assign(evt, ATTR_EX_THROWN, exThrown);
        }

        public bool RemoveExThrown(XEvent evt)
        {
            return Remove(evt, KEY_EX_THROWN);
        }

        public bool ExtractHasData(XLog log)
        {
            return Extract(log, KEY_HAS_DATA, false);
        }

        public IXAttributeBoolean AssignHasData(XLog log, bool hasData)
        {
            return Assign(log, ATTR_HAS_DATA, hasData);
        }

        public bool RemoveHasData(XLog log)
        {
            return Remove(log, KEY_HAS_DATA);
        }

        public bool ExtractHasException(XLog log)
        {
            return Extract(log, KEY_HAS_EXCEPTION, false);
        }

        public IXAttributeBoolean AssignHasException(XLog log, bool hasException)
        {
            return Assign(log, ATTR_HAS_EXCEPTION, hasException);
        }

        public bool RemoveHasException(XLog log)
        {
            return Remove(log, KEY_HAS_EXCEPTION);
        }

        public long ExtractNanotime(XEvent evt)
        {
            return Extract(evt, KEY_NANOTIME, -1L);
        }

        public IXAttributeDiscrete AssignNanotime(XEvent evt, long nanotime)
        {
            return Assign(evt, (XAttributeDiscrete)ATTR_NANOTIME, nanotime);
        }

        public bool RemoveNanotime(XEvent evt)
        {
            return Remove(evt, KEY_NANOTIME);
        }

        public List<XAttribute> ExtractParams(XEvent evt)
        {
            return Extract(evt, KEY_PARAMS, (List<XAttribute>)null);
        }

        public IXAttributeList AssignParams(XEvent evt)
        {
            return Assign(evt, (XAttributeList)ATTR_PARAMS);
        }

        public bool RemoveParams(XEvent evt)
        {
            return Remove(evt, KEY_PARAMS);
        }

        public XAttributeLiteral addParamValue(XAttributeList attribute, string paramValue)
        {
            return AssignToValues(attribute, ATTR_PARAM_VALUE, paramValue);
        }

        public string ExtractReturnValue(XEvent evt)
        {
            return Extract(evt, KEY_RETURN_VALUE, (string)null);
        }

        public XAttributeLiteral AssignReturnValue(XEvent evt, string returnValue)
        {
            return Assign(evt, ATTR_RETURN_VALUE, returnValue);
        }

        public bool RemoveReturnValue(XEvent evt)
        {
            return Remove(evt, KEY_RETURN_VALUE);
        }

        public string ExtractThreadId(XEvent evt)
        {
            return Extract(evt, KEY_THREAD_ID, (string)null);
        }

        public XAttributeLiteral AssignThreadId(XEvent evt, string threadId)
        {
            return Assign(evt, ATTR_THREAD_ID, threadId);
        }

        public bool RemoveThreadId(XEvent evt)
        {
            return Remove(evt, KEY_THREAD_ID);
        }

        public SoftwareEventType ExtractType(XEvent evt)
        {
            return Extract(evt, KEY_TYPE, SoftwareEventType.UNKNOWN);
        }

        public XAttributeLiteral AssignType(XEvent evt, SoftwareEventType type)
        {
            return Assign(evt, ATTR_TYPE, type);
        }

        public bool RemoveType(XEvent evt)
        {
            return Remove(evt, KEY_TYPE);
        }

        public string ExtractValueType(XAttribute attribute)
        {
            return Extract((IXAttributable)attribute, KEY_VALUE_TYPE, (string)null);
        }

        public XAttributeLiteral AssignValueType(XAttribute attribute, string valueTYpe)
        {
            return Assign((IXAttributable)attribute, ATTR_VALUE_TYPE, valueTYpe);
        }

        public bool RemoveValueType(XAttribute attribute)
        {
            return Remove((IXAttributable)attribute, KEY_VALUE_TYPE);
        }


        bool Extract(IXAttributable element, string definedAttribute, bool defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeBoolean)attribute).Value;
        }

        List<XAttribute> Extract(IXAttributable element, string definedAttribute,
                List<XAttribute> defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeList)attribute).GetCollection();
        }

        long Extract(IXAttributable element, string definedAttribute, long defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeDiscrete)attribute).Value;
        }

        string Extract(IXAttributable element, string definedAttribute, string defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ((XAttributeLiteral)attribute).Value;
        }

        SoftwareEventType Extract(IXAttributable element, string definedAttribute,
                SoftwareEventType defaultValue)
        {
            XAttribute attribute = element.GetAttributes()[definedAttribute];

            if (attribute == null)
            {
                return defaultValue;
            }
            return ParseSoftwareEventType(((XAttributeLiteral)attribute).Value);
        }

        XAttributeList Assign(IXAttributable element, XAttributeList definedAttribute)
        {
            XAttributeList attr = (XAttributeList)definedAttribute.Clone();

            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }

        XAttributeLiteral AssignToValues(XAttributeList list, XAttribute definedAttribute, string value)
        {
            if (value != null)
            {
                XAttributeLiteral attr = (XAttributeLiteral)definedAttribute.Clone();

                attr.Value = value;
                list.AddToCollection(attr);

                return attr;
            }
            return null;
        }

        IXAttributeDiscrete Assign(IXAttributable element, XAttributeDiscrete definedAttribute, long value)
        {
            XAttributeDiscrete attr = (XAttributeDiscrete)definedAttribute.Clone();

            attr.Value = value;
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }

        XAttributeLiteral Assign(IXAttributable element, XAttributeLiteral definedAttribute, string value)
        {
            if (value != null)
            {
                XAttributeLiteral attr = (XAttributeLiteral)definedAttribute.Clone();

                attr.Value = value;
                element.GetAttributes().Add(definedAttribute.Key, attr);
                return attr;
            }
            return null;
        }

        XAttributeLiteral Assign(IXAttributable element, XAttributeLiteral definedAttribute,
                SoftwareEventType value)
        {

            XAttributeLiteral attr = (XAttributeLiteral)definedAttribute.Clone();

            attr.Value = GetSoftwareEventTypeName(value);
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;

        }

        IXAttributeBoolean Assign(IXAttributable element, IXAttributeBoolean definedAttribute, bool value)
        {
            XAttributeBoolean attr = (XAttributeBoolean)definedAttribute.Clone();

            attr.Value = value;
            element.GetAttributes().Add(definedAttribute.Key, attr);
            return attr;
        }

        bool Remove(IXAttributable element, string definedAttribute)
        {
            return element.GetAttributes().Remove(definedAttribute);
        }

        string GetSoftwareEventTypeName(SoftwareEventType evtType)
        {
            return Enum.GetName(typeof(SoftwareEventType), evtType).ToLower();
        }

        SoftwareEventType ParseSoftwareEventType(string evtType)
        {
            return (SoftwareEventType)Enum.Parse(typeof(SoftwareEventType), evtType.ToUpper());
        }

    }
}
