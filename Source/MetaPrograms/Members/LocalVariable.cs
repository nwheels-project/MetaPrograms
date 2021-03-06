﻿using MetaPrograms.Expressions;

namespace MetaPrograms.Members
{
    public class LocalVariable : IAssignable
    {
        public IdentifierName Name { get; set; }
        public TypeMember Type { get; set; }
        public bool IsFinal { get; set; }

        public AbstractExpression AsExpression()
        {
            return new LocalVariableExpression {
                Type = this.Type,
                Variable = this
            };
        }

        public void AcceptVisitor(StatementVisitor visitor)
        {
            visitor.VisitReferenceToLocalVariable(this);
        }

        IAssignable IAssignable.AcceptRewriter(StatementRewriter rewriter)
        {
            return this;
        }

        public static implicit operator LocalVariableExpression(LocalVariable source)
        {
            return new LocalVariableExpression {
                Type = source.Type,
                Variable = source
            };
        }
    }
}
