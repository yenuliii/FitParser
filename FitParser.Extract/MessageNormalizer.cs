/*
Gets more information about message names, numbers,
and other information from FitSDKTools/Profile.xlsx.

Takes decoded FIT messages and normalizes them.

The normalizer:
- extracts message information
- extracts field information
- extracts values
- extracts data types and units
- assigns each message a read-only index
*/

namespace FitParser.Extract;

using Dynastream.Fit;
using FitParser.Models;

public sealed class MessageNormalizer
{
    /*
     * Normalizes every raw FIT message and assigns
     * a read-only MessageIndex to each message.
     */
    public MessageCollection NormalizeAll(
        IEnumerable<Mesg> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        MessageCollection collection = new();

        int messageIndex = 1;

        foreach (Mesg message in messages)
        {
            NormalizedMessage normalizedMessage =
                NormalizeMessage(
                    message,
                    messageIndex);

            collection.Add(normalizedMessage);

            messageIndex++;
        }

        return collection;
    }

    /*
     * Normalizes one raw FIT message.
     *
     * This method is private so callers cannot forget
     * to assign a MessageIndex.
     */
    private NormalizedMessage NormalizeMessage(
        Mesg message,
        int messageIndex)
    {
        ArgumentNullException.ThrowIfNull(message);

        List<NormalizedField> fields = new();

        foreach (Field field in message.Fields)
        {
            List<object?> values = new();

            for (int valueIndex = 0;
                 valueIndex < field.GetNumValues();
                 valueIndex++)
            {
                values.Add(
                    field.GetValue(valueIndex));
            }

            string fieldName =
                string.IsNullOrWhiteSpace(field.Name)
                    ? "UnknownField"
                    : field.Name;

            string? clrDataType =
                values
                    .FirstOrDefault(
                        value => value is not null)?
                    .GetType()
                    .FullName;

            Field? profileField =
                Profile.GetField(
                    message.Num,
                    field.Num);

            string? fitDataType = null;
            string? units = null;

            if (profileField is not null)
            {
                fitDataType =
                    profileField.ProfileType.ToString();

                units =
                    string.IsNullOrWhiteSpace(
                        profileField.Units)
                            ? null
                            : profileField.Units;
            }

            fields.Add(
                new NormalizedField(
                    fieldName,
                    field.Num,
                    fitDataType,
                    clrDataType,
                    units,
                    values));
        }

        return new NormalizedMessage(
            message.Name,
            message.Num,
            messageIndex,
            fields);
    }
}