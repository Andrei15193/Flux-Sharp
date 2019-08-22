using System;
using System.Collections.Generic;

namespace FluxBase.Forms.Actions
{
    /// <summary>Represents an action for updating a field.</summary>
    public class UpdateFormAction
    {
        /// <summary>Initializes a new instance of the <see cref="UpdateFormAction"/> class.</summary>
        /// <param name="formName">The field name to update</param>
        /// <param name="updates">The updates to apply on the form.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formName"/> or <paramref name="updates"/> are <c>null</c>.</exception>
        public UpdateFormAction(string formName, IEnumerable<FormUpdate> updates)
        {
            FormName = formName ?? throw new ArgumentNullException(nameof(formName));
            Updates = updates ?? throw new ArgumentNullException(nameof(updates));
            foreach (var update in updates)
                if (update == null)
                    throw new ArgumentException("Cannot contain 'null' updates.", nameof(updates));
        }

        /// <summary>Initializes a new instance of the <see cref="UpdateFormAction"/> class.</summary>
        /// <param name="formName">The field name to update</param>
        /// <param name="updates">The updates to apply on the form.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formName"/> or <paramref name="updates"/> are <c>null</c>.</exception>
        public UpdateFormAction(string formName, params FormUpdate[] updates)
            : this(formName, (IEnumerable<FormUpdate>)updates)
        {
        }

        /// <summary>The form name to which to apply the update.</summary>
        public string FormName { get; }

        /// <summary>The updates to make for a form.</summary>
        public IEnumerable<FormUpdate> Updates { get; }
    }
}