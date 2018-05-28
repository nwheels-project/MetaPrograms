﻿using System.Collections.Immutable;
using System.Reflection;

namespace MetaPrograms.CodeModel.Imperative.Members
{
    public class EventMember : AbstractMember
    {
        public EventMember(
            string name,
            MemberRef<TypeMember> declaringType, 
            MemberStatus status, 
            MemberVisibility visibility, 
            MemberModifier modifier, 
            ImmutableList<AttributeDescription> attributes,
            MemberRef<TypeMember> delegateType, 
            MethodMember adder, 
            MethodMember remover) 
            : base(name, declaringType, status, visibility, modifier, attributes)
        {
            DelegateType = delegateType;
            Adder = adder;
            Remover = remover;
        }

        public EventMember(
            EventMember source, 
            Mutator<string>? name = null, 
            Mutator<MemberRef<TypeMember>>? declaringType = null, 
            Mutator<MemberStatus>? status = null, 
            Mutator<MemberVisibility>? visibility = null, 
            Mutator<MemberModifier>? modifier = null, 
            Mutator<ImmutableList<AttributeDescription>>? attributes = null,
            Mutator<MemberRef<TypeMember>>? delegateType = null,
            Mutator<MethodMember>? adder = null,
            Mutator<MethodMember>? remover = null,
            bool shouldReplaceSource = false) 
            : base(source, name, declaringType, status, visibility, modifier, attributes, shouldReplaceSource)
        {
            DelegateType = delegateType.MutatedOrOriginal(source.DeclaringType);
            Adder = adder.MutatedOrOriginal(source.Adder);
            Remover = remover.MutatedOrOriginal(source.Remover);
        }

        public MemberRef<EventMember> GetRef() => new MemberRef<EventMember>(SelfReference);

        public override AbstractMember WithAttributes(ImmutableList<AttributeDescription> attributes, bool shouldReplaceSource = false)
        {
            return new EventMember(
                source: this,
                attributes: attributes,
                shouldReplaceSource: shouldReplaceSource);
        }

        public override void AcceptVisitor(MemberVisitor visitor)
        {
            base.AcceptVisitor(visitor);

            visitor.VisitEvent(this);

            if (Adder != null)
            {
                Adder.AcceptVisitor(visitor);
            }

            if (Remover != null)
            {
                Remover.AcceptVisitor(visitor);
            }
        }

        public MemberRef<TypeMember> DelegateType { get; }
        public MethodMember Adder { get; }
        public MethodMember Remover { get; }
    }
}
