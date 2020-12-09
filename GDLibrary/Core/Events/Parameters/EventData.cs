using GDLibrary.Enums;
using System;
using System.Collections.Generic;

namespace GDLibrary.Events
{
    /// <summary>
    /// Encapsulates the fields of an event within the game
    /// </summary>
    /// <see cref="GDLibrary.Events.EventDispatcher"/>
    public class EventData
    {
        #region Fields
        private EventCategoryType eventCategoryType;
        private EventActionType eventActionType;
        private object[] parameters;
        #endregion

        #region Properties
        public EventCategoryType EventCategoryType { get => eventCategoryType; set => eventCategoryType = value; }
        public EventActionType EventActionType { get => eventActionType; set => eventActionType = value; }
        public object[] Parameters { get => parameters; set => parameters = value; }
        #endregion

        #region Constructors & Core
        public EventData(EventCategoryType eventCategoryType, 
            EventActionType eventActionType, object[] parameters)
        {
            EventCategoryType = eventCategoryType;
            EventActionType = eventActionType;
            Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            return obj is EventData data &&
                   eventCategoryType == data.eventCategoryType &&
                   eventActionType == data.eventActionType &&
                   EqualityComparer<object[]>.Default.Equals(parameters, data.parameters);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(eventCategoryType, eventActionType, parameters);
        }

        public override string ToString()
        {
            if (parameters == null)
                return eventCategoryType + "," + eventActionType + ", [no params]";
            else
            {
                string parametersAsString = String.Join(",", Array.ConvertAll(parameters, item => item.ToString()));
                return eventCategoryType + "," + eventActionType + "," + parametersAsString;
            }
        }
        #endregion
    }

}
