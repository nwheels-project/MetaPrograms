﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MetaPrograms.Members;
using Microsoft.CodeAnalysis;

namespace MetaPrograms
{
    public class CodeModelBuilder
    {
        private readonly ImperativeCodeModel _codeModel;
        private readonly ImmutableArray<Compilation> _compilations;
        private readonly ImmutableDictionary<SyntaxTree, Compilation> _compilationBySyntaxTree;

        public CodeModelBuilder(Compilation mainCompilation)
            : this(new[] { mainCompilation })
        {
        }

        public CodeModelBuilder(IEnumerable<Compilation> knownCompilations)
        {
            _codeModel = new ImperativeCodeModel();
            _compilations = WithReferencedCompilations(knownCompilations.ToArray()).ToImmutableArray();

            _compilationBySyntaxTree = _compilations
                .SelectMany(
                    compilation => compilation.SyntaxTrees
                        .Select(tree => new KeyValuePair<SyntaxTree, Compilation>(tree, compilation)))
                .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        //public void BindMember<TBinding>(MemberRef<AbstractMember> member, TBinding binding)
        //    where TBinding : class
        //{
        //    member.Bindings.Add(binding);
        //    _memberByBinding[binding] = member;
        //}

        public void RegisterMember(AbstractMember member, bool isTopLevel)
        {
            _codeModel.Add(member, isTopLevel);
        }

        //public bool TryGetMember<TMember, TBinding>(TBinding binding, out TMember member)
        //    where TMember : AbstractMember
        //    where TBinding : class
        //{
        //    if (_memberByBinding.TryGetValue(binding, out AbstractMember abstractMember))
        //    {
        //        member = (TMember)abstractMember;
        //        return true;
        //    }

        //    member = default;
        //    return false;
        //}

        // public MemberRef<TMember> GetMember<TMember, TBinding>(TBinding binding)
        //     where TMember : AbstractMember
        //     where TBinding : class
        // {
        //     if (_memberByBinding.TryGetValue(binding, out MemberRef<AbstractMember> existingMember))
        //     {
        //         return existingMember.AsRef<TMember>();
        //     }
        //
        //     return MemberRef<TMember>.Null;
        //     // throw new KeyNotFoundException(
        //     //     $"{typeof(TMember).Name} with binding '{typeof(TBinding).Name}={binding}' could not be found.");
        // }

        public TMember TryGetMember<TMember>(object binding)
            where TMember : AbstractMember
        {
            return _codeModel.TryGet<TMember>(binding);
        }

        public Compilation GetCompilation(SyntaxTree syntax)
        {
            if (_compilationBySyntaxTree.TryGetValue(syntax, out Compilation compilation))
            {
                return compilation;
            }

            throw new KeyNotFoundException(
                $"Syntax tree with path '{syntax.FilePath}' could not be found in any of the compilations.");
        }

        public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            var compilation = GetCompilation(syntaxTree);
            return compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
        }

        public SemanticModel GetSemanticModel(SyntaxNode syntax)
        {
            return GetSemanticModel(syntax.SyntaxTree);
        }

        //public TMember GetOrAddMember<TMember, TBinding>(TBinding binding, Func<TMember> memberFactory)
        //    where TMember : AbstractMember
        //    where TBinding : class
        //{
        //    if (_memberByBinding.TryGetValue(binding, out AbstractMember existingMember))
        //    {
        //        return (TMember)existingMember;
        //    }

        //    var newMember = memberFactory();
        //    BindMember(newMember, binding);

        //    return newMember;
        //}

        public IEnumerable<AbstractMember> GetRgisteredMembers() => new HashSet<AbstractMember>(_codeModel.MembersByBndings.Values);

        public IEnumerable<AbstractMember> GetTopLevelMembers() => _codeModel.TopLevelMembers;

        public ImmutableArray<Compilation> GetCompilations() => _compilations;

        public ImperativeCodeModel GetCodeModel() => _codeModel;

        private static IEnumerable<Compilation> WithReferencedCompilations(Compilation[] knownCompilations)
        {
            var dedup = new HashSet<Compilation>();

            return 
                knownCompilations
                .Concat(
                    knownCompilations.SelectMany(compilation => compilation.References.OfType<CompilationReference>())
                        .Select(r => r.Compilation)
                )
                .Where(dedup.Add); // Distinct which preserves original ordering 
        }
    }
}