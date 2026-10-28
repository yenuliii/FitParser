/*
GET more information about message names, numbers and other information
from FitSDKTools/Profile.xlsx
*/

using System.Collections.ObjectModel;
using Dynastream.Fit;

namespace FitParser.Core;

public sealed class MessageNormalizer
{
    public NormalizedMessage Normalize(Mesg message)
    {
        ArgumentNullException.ThrowIfNull(message);

        List<NormalizedField> fields = new();

        foreach (Field field in message.Fields)
        {
            List<object?> values = new();

            for (int index = 0;
                 index < field.GetNumValues();
                 index++)
            {
                values.Add(field.GetValue(index));
            }

            string fieldName = string.IsNullOrWhiteSpace(field.Name)
                ? "UnknownField"
                : field.Name;

            string? clrDataType = values
                .FirstOrDefault(value => value is not null)?
                .GetType()
                .FullName;

            Field? profileField =
                Profile.GetField(message.Num, field.Num);

            string? fitDataType = null;

            if (profileField is not null)
            {
                fitDataType =
                    profileField.ProfileType.ToString();
            }

            fields.Add(
                new NormalizedField(
                    fieldName,
                    field.Num,
                    fitDataType,
                    clrDataType,
                    values.AsReadOnly()));
        }

        return new NormalizedMessage(
            message.Name,
            message.Num,
            fields.AsReadOnly());
    }

}

public sealed class NormalizedMessage
{
    public string MessageName { get; }

    public int MessageNumber { get; }

    public IReadOnlyList<NormalizedField> Fields { get; }

    public NormalizedMessage(
        string messageName,
        int messageNumber,
        IReadOnlyList<NormalizedField> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageName);
        ArgumentNullException.ThrowIfNull(fields);

        MessageName = messageName;
        MessageNumber = messageNumber;

        Fields = new ReadOnlyCollection<NormalizedField>(
            fields.ToList());
    }
}

public sealed class NormalizedField
{
    public string FieldName { get; }

    public int FieldNumber { get; }

    public string? FitDataType { get; }

    public string? ClrDataType { get; }

    public IReadOnlyList<object?> Values { get; }

    public NormalizedField(
        string fieldName,
        int fieldNumber,
        string? fitDataType,
        string? clrDataType,
        IReadOnlyList<object?> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentNullException.ThrowIfNull(values);

        FieldName = fieldName;
        FieldNumber = fieldNumber;
        FitDataType = fitDataType;
        ClrDataType = clrDataType;

        Values = new ReadOnlyCollection<object?>(
            values.ToList());
    }
}