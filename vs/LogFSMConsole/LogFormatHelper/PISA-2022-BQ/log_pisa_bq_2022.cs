using System.Xml.Serialization;

[System.Xml.Serialization.XmlRootAttribute("log", Namespace = "", IsNullable = false)]
public class log_pisa_bq_2022
{ 
    private log_pisa_bq_2022_itemGroup[] ItemGroupField;

    private string isoField;

    private string questionnaireField;

    private string userField;
      
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Iso
    {
        get
        {
            return this.isoField;
        }
        set
        {
            this.isoField = value;
        }
    }
     
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Questionnaire
    {
        get
        {
            return this.questionnaireField;
        }
        set
        {
            this.questionnaireField = value;
        }
    }
     
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string User
    {
        get
        {
            return this.userField;
        }
        set
        {
            this.userField = value;
        }
    }

    [System.Xml.Serialization.XmlElementAttribute("ItemGroup")]
    public log_pisa_bq_2022_itemGroup[] ItemGroup
    {
        get
        {
            return this.ItemGroupField;
        }
        set
        {
            this.ItemGroupField = value;
        }
    }
}

 
public partial class log_pisa_bq_2022_itemGroup
{
    private long epochField;

    private bool epochFieldSpecified;

    private log_pisa_bq_2022_Event[] userEventsField;

    private string codeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    [System.Xml.Serialization.XmlArrayItemAttribute("event", IsNullable = false)]
    public log_pisa_bq_2022_Event[] userEvents
    {
        get
        {
            return this.userEventsField;
        }
        set
        {
            this.userEventsField = value;
        }
    }

     
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public long epoch
    {
        get
        {
            return this.epochField;
        }
        set
        {
            this.epochField = value;
        }
    }
     
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool epochSpecified
    {
        get
        {
            return this.epochFieldSpecified;
        }
        set
        {
            this.epochFieldSpecified = value;
        }
    }
}
 
public partial class log_pisa_bq_2022_Event
{

    private string[] itemsField;

    private ItemsChoiceType[] itemsElementNameField;

    private string typeField;

    private long epochField;

    private bool epochFieldSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("context", typeof(string))]
    [System.Xml.Serialization.XmlElementAttribute("id", typeof(string))]
    [System.Xml.Serialization.XmlElementAttribute("value", typeof(string))]
    [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
    public string[] Items
    {
        get
        {
            return this.itemsField;
        }
        set
        {
            this.itemsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public ItemsChoiceType[] ItemsElementName
    {
        get
        {
            return this.itemsElementNameField;
        }
        set
        {
            this.itemsElementNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public long epoch
    {
        get
        {
            return this.epochField;
        }
        set
        {
            this.epochField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool epochSpecified
    {
        get
        {
            return this.epochFieldSpecified;
        }
        set
        {
            this.epochFieldSpecified = value;
        }
    }
}
 